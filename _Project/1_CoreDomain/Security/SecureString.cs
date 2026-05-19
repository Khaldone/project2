// Assets/_Project/CoreDomain/Security/SecureString.cs
using System;
using System.Text;

/// <summary>
/// In-memory obfuscation for string values via per-byte XOR.
/// Protects against GameGuardian-style memory scanners that search for known UTF-8 patterns.
/// The key is regenerated on every write to prevent static-key attacks.
/// </summary>
public struct SecureString
{
    private byte[] _cryptoKey;
    private byte[] _obfuscatedBytes;

    /// <summary>
    /// Gets or sets the real string value. The setter re-keys on every write.
    /// </summary>
    public string Value
    {
        get
        {
            if (_obfuscatedBytes == null || _cryptoKey == null)
                return null;

            byte[] decrypted = new byte[_obfuscatedBytes.Length];
            for (int i = 0; i < _obfuscatedBytes.Length; i++)
            {
                decrypted[i] = (byte)(_obfuscatedBytes[i] ^ _cryptoKey[i % _cryptoKey.Length]);
            }
            return Encoding.UTF8.GetString(decrypted);
        }
        set
        {
            if (value == null)
            {
                _cryptoKey = null;
                _obfuscatedBytes = null;
                return;
            }

            byte[] rawBytes = Encoding.UTF8.GetBytes(value);
            var rng = new Random();

            // Key length matches data length for maximum entropy
            _cryptoKey = new byte[rawBytes.Length];
            rng.NextBytes(_cryptoKey);

            _obfuscatedBytes = new byte[rawBytes.Length];
            for (int i = 0; i < rawBytes.Length; i++)
            {
                _obfuscatedBytes[i] = (byte)(rawBytes[i] ^ _cryptoKey[i]);
            }
        }
    }

    public SecureString(string initialValue)
    {
        _cryptoKey = null;
        _obfuscatedBytes = null;
        Value = initialValue;
    }
}
