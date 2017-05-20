using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PicuCalendars.Security
{
    /// <summary>
    /// Simple encryption/decryption using a random initialization vector
    /// and prepending it to the crypto text.
    /// </summary>
    /// <remarks>Based on multiple answers in http://stackoverflow.com/questions/165808/simple-two-way-encryption-for-c-sharp </remarks>
    public sealed class SimpleAes : IDisposable
    {
        /// <summary>
        ///     Initialization vector length in bytes.
        /// </summary>
        private const int _ivBytes = 16;

        /// <summary>
        ///     Must be exactly 16, 24 or 32 bytes long.
        /// </summary>

        private readonly Encoding _encoder;
        private readonly RijndaelManaged _rijndael;

        public SimpleAes(string base64Key = "xA9lRSIIRiVevlJQyiBK7gcBV3CDSrso9RYOcrnvU2o=") : this(Convert.FromBase64String(base64Key))
        { }

        public SimpleAes(byte[] key)
        {
            if (!(new[] { 16,24,32 }).Any(l => l == key.Length))
            {
                throw new ArgumentException($"Invalid key length:{key.Length} - must be 16, 24 or 32 bytes long");
            }
            _rijndael = new RijndaelManaged { Key = key };
            _encoder = Encoding.UTF8;
        }

        public string Decrypt(string encrypted)
        {
            return _encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(_encoder.GetBytes(unencrypted)));
        }

        private byte[] Decrypt(byte[] buffer)
        {
            byte[] iv = buffer.Take(_ivBytes).ToArray();
            using (ICryptoTransform decryptor = _rijndael.CreateDecryptor(_rijndael.Key, iv))
            {
                return decryptor.TransformFinalBlock(buffer,_ivBytes, buffer.Length - _ivBytes);
            }
        }

        private byte[] Encrypt(byte[] buffer)
        {
            _rijndael.GenerateIV();
            using (var encryptor = _rijndael.CreateEncryptor(_rijndael.Key, _rijndael.IV))
            {
                byte[] inputBuffer = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                return _rijndael.IV.Concat(inputBuffer).ToArray();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        private bool _disposed;
        void Dispose(bool disposing)
        { // would be protected virtual if not sealed 
            if (disposing && !_disposed)
            { // only run this logic when Dispose is called
                // and anything else that touches managed objects
                _rijndael.Dispose();
                //_encryptor.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
    public static class CryptoUtilities
    {
        public static byte[] GenerateKey(int keyLength = 64)
        {
            var returnVar = new Byte[keyLength];
            using (var c = new RNGCryptoServiceProvider())
            {
                c.GetBytes(returnVar);
            }
            return returnVar;
        }

    }
}
