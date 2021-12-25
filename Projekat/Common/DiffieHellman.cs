using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public class DiffieHellman : IDisposable
    {
        private Aes aes = null;
        private ECDiffieHellmanCng diffieHellman = null;

        private readonly byte[] publicKey;

        public DiffieHellman()
        {
            this.aes = new AesCryptoServiceProvider();
            this.aes.Mode = CipherMode.CBC;             // requested by assigment

            this.diffieHellman = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            // This is the public key we will send to the other party
            this.publicKey = this.diffieHellman.PublicKey.ToByteArray();
        }

        public byte[] PublicKey
        {
            get { return this.publicKey; }
        }

        public byte[] IV
        {
            get { return this.aes.IV; }
        }

        public byte[] Encrypt(byte[] publicKey, string secretMessage)
        {
            byte[] encryptedMessage;
            var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            var derivedKey = this.diffieHellman.DeriveKeyMaterial(key);         // Common secret, everybody knows it

            this.aes.Key = derivedKey;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var encryptor = this.aes.CreateEncryptor())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] cipherTextMessage = Encoding.UTF8.GetBytes(secretMessage);
                        cryptoStream.Write(cipherTextMessage, 0, cipherTextMessage.Length);
                    }
                }
                encryptedMessage = memoryStream.ToArray();
            }

            return encryptedMessage;
        }

        public string Decrypt(byte[] publicKey, byte[] encryptedMessage, byte[] iv)
        {
            string decryptedMessage;
            var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            var derivedKey = this.diffieHellman.DeriveKeyMaterial(key);

            this.aes.Key = derivedKey;
            this.aes.IV = iv;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var decryptor = this.aes.CreateDecryptor())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedMessage, 0, encryptedMessage.Length);
                    }
                }
                decryptedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            return decryptedMessage;
        }

        // Clean up unmanaged resources
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.aes != null)
                    this.aes.Dispose();

                if (this.diffieHellman != null)
                    this.diffieHellman.Dispose();
            }
        }
    }
}
