// Assets/_Project/CoreDomain/Security/SecureBool.cs
using System;

/// <summary>
/// In-memory obfuscation for boolean values via XOR on a randomized int.
/// A memory scanner looking for 0x00/0x01 will not find the real value.
/// The key is regenerated on every write.
/// </summary>
public struct SecureBool
{
    private int _cryptoKey;
    private int _obfuscatedValue;

    /// <summary>
    /// Gets or sets the real bool value. The setter re-keys on every write.
    /// </summary>
    public bool Value
    {
        get => (_obfuscatedValue ^ _cryptoKey) == 1;
        set
        {
            _cryptoKey = new Random().Next();
            _obfuscatedValue = (value ? 1 : 0) ^ _cryptoKey;
        }
    }

    public SecureBool(bool initialValue)
    {
        _cryptoKey = new Random().Next();
        _obfuscatedValue = (initialValue ? 1 : 0) ^ _cryptoKey;
    }
}
