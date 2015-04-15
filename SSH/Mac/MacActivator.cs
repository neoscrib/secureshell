using SSH.IO;
using System;

namespace SSH.Mac
{
    class MacActivator
    {
        string identifier;
        Type hashType;
        byte[] sharedSecret;
        byte[] exchangeHash;
        byte[] key;

        public MacActivator(string identifier, Type hashType, byte[] sharedSecret, byte[] exchangeHash, byte[] key)
        {
            this.identifier = identifier;
            this.hashType = hashType;
            this.sharedSecret = sharedSecret;
            this.exchangeHash = exchangeHash;
            this.key = key;
        }

        public MacWriter Create()
        {
            return MacWriter.Create(this.identifier, this.hashType, this.sharedSecret, this.exchangeHash, this.key);
        }
    }
}
