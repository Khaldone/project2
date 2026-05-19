// Assets/_Project/CoreDomain/Security/SecureFloat.cs
using System;

/// <summary>
/// In-memory obfuscation for float values via XOR on the raw IEEE-754 bits.
/// Protects against GameGuardian-style memory scanners that search for known float patterns.
/// </summary>
public struct SecureFloat
{
    private int _cryptoKey;
    private int _obfuscatedBits;

    /// <summary>
    /// Gets or sets the real float value. The setter re-keys on every write.
    /// </summary>
    public float Value
    {
        get
        {
            int rawBits = _obfuscatedBits ^ _cryptoKey;
            return BitConverter.Int32BitsToSingle(rawBits);
        }
        set
        {
            _cryptoKey = new Random().Next();
            int rawBits = BitConverter.SingleToInt32Bits(value);
            _obfuscatedBits = rawBits ^ _cryptoKey;
        }
    }

    public SecureFloat(float initialValue)
    {
        _cryptoKey = new Random().Next();
        int rawBits = BitConverter.SingleToInt32Bits(initialValue);
        _obfuscatedBits = rawBits ^ _cryptoKey;
    }
}
