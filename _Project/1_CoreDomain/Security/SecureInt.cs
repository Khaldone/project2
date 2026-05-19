// Assets/_Project/CoreDomain/Security/SecureInt.cs
using System;
public struct SecureInt
{
    private int _cryptoKey;
    private int _obfuscatedValue;


    // When you set the value, it immediately scrambles it
    public int Value
    {
        get => _obfuscatedValue ^ _cryptoKey; // XOR decrypts it
        set
        {
            _cryptoKey = new Random().Next(); // Generate a new random key
            _obfuscatedValue = value ^ _cryptoKey; // XOR encrypts it
        }
    }


    public SecureInt(int initialValue)
    {
        _cryptoKey = new Random().Next();
        _obfuscatedValue = initialValue ^ _cryptoKey;
    }
}