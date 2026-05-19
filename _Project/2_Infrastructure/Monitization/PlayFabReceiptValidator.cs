using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using Billiards.CoreDomain.Monetization;

namespace Billiards.Infrastructure.Monetization
{
    public class PlayFabReceiptValidator : IReceiptValidator
    {
        [Serializable]
        private class UnityIAPReceipt
        {
            public string Store;
            public string TransactionID;
            public string Payload;
        }

        [Serializable]
        private class GooglePayload
        {
            public string json;
            public string signature;
        }

        public UniTask<bool> ValidateReceiptAndGrantItemsAsync(string productId, PurchaseReceipt receipt)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            
            try
            {
                var unityReceipt = JsonUtility.FromJson<UnityIAPReceipt>(receipt.ReceiptData);

                if (unityReceipt.Store == "GooglePlay")
                {
                    var googlePayload = JsonUtility.FromJson<GooglePayload>(unityReceipt.Payload);
                    var request = new ValidateGooglePlayPurchaseRequest
                    {
                        CurrencyCode = receipt.CurrencyCode,
                        PurchasePrice = (uint)receipt.PriceInCents,
                        ReceiptJson = googlePayload.json,
                        Signature = googlePayload.signature
                    };

                    PlayFabClientAPI.ValidateGooglePlayPurchase(request,
                        result => tcs.TrySetResult(true),
                        error =>
                        {
                            Debug.LogError($"[PlayFabReceiptValidator] Google Validation Failed: {error.GenerateErrorReport()}");
                            tcs.TrySetResult(false);
                        });
                }
                else if (unityReceipt.Store == "AppleAppStore" || unityReceipt.Store == "MacAppStore")
                {
                    var request = new ValidateIOSReceiptRequest
                    {
                        CurrencyCode = receipt.CurrencyCode,
                        PurchasePrice = receipt.PriceInCents,
                        ReceiptData = unityReceipt.Payload
                    };

                    PlayFabClientAPI.ValidateIOSReceipt(request,
                        result => tcs.TrySetResult(true),
                        error =>
                        {
                            Debug.LogError($"[PlayFabReceiptValidator] iOS Validation Failed: {error.GenerateErrorReport()}");
                            tcs.TrySetResult(false);
                        });
                }
                else
                {
                    // For Unity Editor fake store
                    if (Application.isEditor)
                    {
                        Debug.Log("[PlayFabReceiptValidator] EDITOR MODE: Bypassing PlayFab Server Validation. Simulating success.");
                        Debug.Log($"[PlayFabReceiptValidator] --- SIMULATED RECEIPT PAYLOAD ---\nStore: {unityReceipt.Store}\nTransaction ID: {unityReceipt.TransactionID}\nPayload: {unityReceipt.Payload}\n----------------------------------");
                        
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayFabReceiptValidator] Unsupported store: {unityReceipt.Store}");
                        tcs.TrySetResult(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayFabReceiptValidator] Receipt parsing failed: {ex.Message}");
                tcs.TrySetResult(false);
            }

            return tcs.Task;
        }
    }
}
