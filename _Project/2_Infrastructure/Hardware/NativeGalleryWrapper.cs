using System;
using System.IO;
using Billiards.CoreDomain.Hardware;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Billiards.Infrastructure.Hardware
{
    public class NativeGalleryWrapper : IGalleryService
    {
        public async UniTask<byte[]> PickAndCompressImageAsync(int maxSize = 512, int jpgQuality = 85)
        {
            var tcs = new UniTaskCompletionSource<byte[]>();

            // Must be called from the main thread.
            // This version of NativeGallery handles permissions internally.
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning("[NativeGalleryWrapper] No image selected from the gallery or action cancelled.");
                    tcs.TrySetResult(null);
                    return;
                }

                try
                {
                    // NativeGallery can automatically downscale the image to 'maxSize' when loading it from disk
                    // This is much more memory efficient than loading the full image and downscaling in Unity
                    Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize, false);
                    if (texture == null)
                    {
                        Debug.LogError($"[NativeGalleryWrapper] Failed to load image at path: {path}");
                        tcs.TrySetResult(null);
                        return;
                    }

                    // Encode to JPG for smaller file size (ideal for avatars)
                    byte[] imageBytes = texture.EncodeToJPG(jpgQuality);

                    // Clean up the Texture2D from memory immediately
                    UnityEngine.Object.Destroy(texture);

                    tcs.TrySetResult(imageBytes);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NativeGalleryWrapper] Exception during image processing: {ex.Message}");
                    tcs.TrySetResult(null);
                }
            }, "Select a Profile Photo", "image/*");

            return await tcs.Task;
        }

        public async UniTask<GalleryPickResult> PickImageLosslessAsync(int maxSize = 1024, long maxFileSizeBytes = 25L * 1024 * 1024)
        {
            var tcs = new UniTaskCompletionSource<GalleryPickResult>();

            NativeGallery.GetImageFromGallery((path) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning("[NativeGalleryWrapper] No image selected from the gallery or action cancelled.");
                    tcs.TrySetResult(GalleryPickResult.Cancelled());
                    return;
                }

                // File-size cap: check BEFORE decode so an oversized image cannot
                // spike memory by loading a huge raw bitmap on low-RAM devices.
                long fileSize;
                try
                {
                    fileSize = new FileInfo(path).Length;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NativeGalleryWrapper] Could not stat file at {path}: {ex.Message}");
                    tcs.TrySetResult(GalleryPickResult.LoadFailed());
                    return;
                }

                if (fileSize > maxFileSizeBytes)
                {
                    Debug.LogWarning($"[NativeGalleryWrapper] Selected image ({fileSize / (1024 * 1024)} MB) exceeds the {maxFileSizeBytes / (1024 * 1024)} MB cap — rejecting before decode.");
                    tcs.TrySetResult(GalleryPickResult.TooLarge(fileSize));
                    return;
                }

                try
                {
                    // Downsample on disk to keep the in-memory texture bounded, but skip JPG
                    // quantisation entirely — moderation accuracy depends on pixel fidelity.
                    Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize, false);
                    if (texture == null)
                    {
                        Debug.LogError($"[NativeGalleryWrapper] Failed to load image at path: {path}");
                        tcs.TrySetResult(GalleryPickResult.LoadFailed());
                        return;
                    }

                    byte[] pngBytes = texture.EncodeToPNG();
                    UnityEngine.Object.Destroy(texture);

                    tcs.TrySetResult(GalleryPickResult.Loaded(pngBytes, fileSize));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NativeGalleryWrapper] Exception during lossless image processing: {ex.Message}");
                    tcs.TrySetResult(GalleryPickResult.LoadFailed());
                }
            }, "Select a Profile Photo", "image/*");

            return await tcs.Task;
        }
    }
}