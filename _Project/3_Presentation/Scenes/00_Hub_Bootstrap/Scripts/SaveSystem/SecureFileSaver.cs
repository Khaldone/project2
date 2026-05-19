// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/SaveSystem/SecureFileSaver.cs
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class SecureFileSaver : ILocalSaveService
{
    // In a real production environment, you would generate a unique device-specific
    // key here, rather than hardcoding it, so a player can't send their save file to a friend.
    private readonly byte[] _encryptionKey = Encoding.UTF8.GetBytes("YOUR_32_BYTE_SECURE_SECRET_KEY!!");
    private readonly byte[] _initializationVector = Encoding.UTF8.GetBytes("YOUR_16_BYTE_IV!");


    public async UniTask SaveDataAsync<T>(string key, T data)
    {
        string filePath = GetFilePath(key);
        // JsonUtility cannot serialize raw primitives (string, int, etc.).
        // For strings we encrypt the value directly; for everything else we use JSON.
        string plainText = data is string s ? s : JsonUtility.ToJson(data);

        byte[] encryptedData = await UniTask.RunOnThreadPool(() => Encrypt(plainText));
        await File.WriteAllBytesAsync(filePath, encryptedData);
    }


    public async UniTask<T> LoadDataAsync<T>(string key, T defaultValue = default)
    {
        string filePath = GetFilePath(key);

        if (!File.Exists(filePath)) return defaultValue;

        try
        {
            byte[] encryptedData = await File.ReadAllBytesAsync(filePath);
            string decrypted = await UniTask.RunOnThreadPool(() => Decrypt(encryptedData));

            // Mirror the save logic: strings are stored raw, everything else is JSON.
            if (typeof(T) == typeof(string))
                return (T)(object)decrypted;

            return JsonUtility.FromJson<T>(decrypted);
        }
        catch (Exception ex)
        {
            Debug.LogError($"AAA Pipeline: Save file corrupted or tampered with! {ex.Message}");
            return defaultValue;
        }
    }


    public void DeleteData(string key) { /* ... File.Delete(GetFilePath(key)) ... */ }


    private string GetFilePath(string key) => Path.Combine(Application.persistentDataPath, $"{key}.dat");


    // --- AES Cryptography ---
    private byte[] Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _initializationVector;


        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new MemoryStream();
        using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (StreamWriter sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return ms.ToArray();
    }


    private string Decrypt(byte[] cipherText)
    {
        using Aes aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _initializationVector;


        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream ms = new MemoryStream(cipherText);
        using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
