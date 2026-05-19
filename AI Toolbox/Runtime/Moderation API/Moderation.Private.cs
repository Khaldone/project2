using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace AiToolbox {
public static partial class Moderation {
    internal struct ModerationRequest {
        public object input;
        public string model;
    }

    [System.Serializable]
    internal class ImageUrlInput {
        [JsonProperty("type")]
        public string type = "image_url";

        [JsonProperty("image_url")]
        public ImageUrl imageUrl;

        public ImageUrlInput(string base64Data, string mimeType = "image/jpeg") {
            imageUrl = new ImageUrl($"data:{mimeType};base64,{base64Data}");
        }
    }

    [System.Serializable]
    internal class ImageUrl {
        [JsonProperty("url")]
        public string url;

        public ImageUrl(string url) => this.url = url;
    }

    private static readonly List<RequestRecord> RequestRecords = new();

    private static Action QuickRequestBlocking(string text, ModerationParameters parameters,
                                               Action<ModerationResponse> completeCallback,
                                               Action<long, string> failureCallback) {
        Debug.Assert(parameters != null, "Parameters cannot be null.");
        Debug.Assert(!string.IsNullOrEmpty(parameters!.apiKey), "API key cannot be null or empty.");
        Debug.Assert(!string.IsNullOrEmpty(text), "Input text cannot be null or empty.");

        // Throttle.
        if (parameters.throttle > 0) {
            var requestCount = RequestRecords.Count;
            if (requestCount >= parameters.throttle) {
                failureCallback?.Invoke((long)ErrorCodes.ThrottleExceeded,
                                        $"Too many requests. Maximum allowed: {parameters.throttle}.");
                return () => { };
            }
        }

        var requestObject = new ModerationRequest {
            input = text,
            model = "omni-moderation-latest"
        };

        var requestRecord = new RequestRecord();
        var requestJson = JsonConvert.SerializeObject(requestObject);
        Debug.Log($"[Moderation] Request JSON: {requestJson}");
        var webRequest = GetWebRequest(requestJson, parameters, failureCallback, requestRecord);
        var cancelCallback = new Action(() => {
            try {
                webRequest?.Abort();
                webRequest?.Dispose();
                RequestRecords.Remove(requestRecord);
            }
            catch (Exception) {
                // If the request is aborted, accessing the error property will throw an exception.
            }
        });
        requestRecord.SetCancelCallback(cancelCallback);
        RequestRecords.Add(requestRecord);

        webRequest.SendWebRequest().completed += _ => {
            RequestRecords.Remove(requestRecord);
            Application.quitting -= cancelCallback;

            bool isErrorResponse;
            try {
                isErrorResponse = !string.IsNullOrEmpty(webRequest.error);
            }
            catch (Exception) {
                // If the request is aborted, accessing the error property will throw an exception.
                return;
            }

            if (isErrorResponse) {
                Debug.LogError($"[Moderation] API Error - Code: {webRequest.responseCode}, Error: {webRequest.error}");
                Debug.LogError($"[Moderation] Response Text: {webRequest.downloadHandler.text}");
                failureCallback?.Invoke(webRequest.responseCode, webRequest.error);
                return;
            }

            var response =
                Newtonsoft.Json.JsonConvert.DeserializeObject<ModerationResponse>(webRequest.downloadHandler.text);
            completeCallback?.Invoke(response);
            webRequest.Dispose();
        };

        Application.quitting += cancelCallback;
        return cancelCallback;
    }

    private static Action QuickRequestBlockingImage(Texture2D image, ModerationParameters parameters,
                                                     Action<ModerationResponse> completeCallback,
                                                     Action<long, string> failureCallback) {
        Debug.Assert(parameters != null, "Parameters cannot be null.");
        Debug.Assert(!string.IsNullOrEmpty(parameters!.apiKey), "API key cannot be null or empty.");
        Debug.Assert(image != null, "Image cannot be null.");

        if (parameters.throttle > 0) {
            var requestCount = RequestRecords.Count;
            if (requestCount >= parameters.throttle) {
                failureCallback?.Invoke((long)ErrorCodes.ThrottleExceeded,
                                        $"Too many requests. Maximum allowed: {parameters.throttle}.");
                return () => { };
            }
        }

        Texture2D readableTexture = DecompressTexture(image);
        byte[] pngData = readableTexture.EncodeToPNG();
        UnityEngine.Object.Destroy(readableTexture);

        if (pngData == null || pngData.Length == 0) {
            failureCallback?.Invoke((long)ErrorCodes.Unknown, "Failed to encode image to PNG.");
            return () => { };
        }

        var base64Data = System.Convert.ToBase64String(pngData);
        var imageInput = new ImageUrlInput(base64Data, "image/png");

        var requestObject = new ModerationRequest {
            input = new[] { imageInput },
            model = "omni-moderation-latest"
        };

        var requestRecord = new RequestRecord();
        var requestJson = JsonConvert.SerializeObject(requestObject);
        Debug.Log($"[Moderation] Image Request JSON (truncated): {requestJson.Substring(0, Mathf.Min(200, requestJson.Length))}...");
        var webRequest = GetWebRequest(requestJson, parameters, failureCallback, requestRecord);
        var cancelCallback = new Action(() => {
            try {
                webRequest?.Abort();
                webRequest?.Dispose();
                RequestRecords.Remove(requestRecord);
            }
            catch (Exception) {
                // If the request is aborted, accessing the error property will throw an exception.
            }
        });
        requestRecord.SetCancelCallback(cancelCallback);
        RequestRecords.Add(requestRecord);

        webRequest.SendWebRequest().completed += _ => {
            RequestRecords.Remove(requestRecord);
            Application.quitting -= cancelCallback;

            bool isErrorResponse;
            try {
                isErrorResponse = !string.IsNullOrEmpty(webRequest.error);
            }
            catch (Exception) {
                return;
            }

            if (isErrorResponse) {
                Debug.LogError($"[Moderation] API Error - Code: {webRequest.responseCode}, Error: {webRequest.error}");
                Debug.LogError($"[Moderation] Response Text: {webRequest.downloadHandler.text}");
                failureCallback?.Invoke(webRequest.responseCode, webRequest.error);
                return;
            }

            var response =
                JsonConvert.DeserializeObject<ModerationResponse>(webRequest.downloadHandler.text);
            completeCallback?.Invoke(response);
            webRequest.Dispose();
        };

        Application.quitting += cancelCallback;
        return cancelCallback;
    }

    private static IEnumerator GetRemoteConfig(ModerationParameters parameters, Action<long, string> failureCallback) {
        var apiKeySet = false;
        var task = RemoteKeyService.GetApiKey(parameters.apiKeyRemoteConfigKey, s => {
            parameters.apiKeyEncryption = ApiKeyEncryption.None;
            parameters.apiKey = s;
            apiKeySet = true;
        }, (errorCode, error) => {
            failureCallback?.Invoke(errorCode, error);
            apiKeySet = true;
        });

        yield return new WaitUntil(() => task.IsCompleted && apiKeySet);

        if (task.IsFaulted) {
            failureCallback?.Invoke((long)ErrorCodes.RemoteConfigConnectionFailure,
                                    "Failed to retrieve API key from remote config.");
        }
    }

    private static Texture2D DecompressTexture(Texture2D source) {
        var renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTexture);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        var readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTexture;
    }

    private static UnityWebRequest GetWebRequest(string requestJson, ModerationParameters parameters,
                                                 Action<long, string> failureCallback, RequestRecord requestRecord) {
        var url = "https://api.openai.com/v1/moderations";
        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestJson));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = parameters.timeout;

        try {
            var apiKey = parameters.apiKey;
            var isEncrypted = parameters.apiKeyEncryption == ApiKeyEncryption.LocallyEncrypted;
            if (isEncrypted) {
                apiKey = Key.B(apiKey, parameters.apiKeyEncryptionPassword);
            }

            var maskedKey = apiKey.Length > 10 ? apiKey.Substring(0, 10) + "..." : apiKey;
            Debug.Log($"[Moderation] Using API key: {maskedKey}");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        }
        catch (Exception e) {
            failureCallback?.Invoke((long)ErrorCodes.Unknown, e.Message);
            // 0d344651-d8d3-46d2-b91c-031a0a12d4e8
            RequestRecords.Remove(requestRecord);
        }

        return request;
    }
}
}