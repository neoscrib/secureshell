using System;
using System.Security.Cryptography;

namespace SSH.Identity
{
    public interface IIdentityFile
    {
        string Path { get; set; }
        bool Encrypted { get; set; }
        AsymmetricAlgorithm Algorithm { get; set; }
        string AlgorithmName { get; }
        Func<IIdentityFile, string> PassphraseFunction { get; set; }
        byte[] PublicKey { get; }
        byte[] Process();
        void Decrypt();
        void ExtractParameters();
        byte[] Sign(byte[] data);
    }
}
