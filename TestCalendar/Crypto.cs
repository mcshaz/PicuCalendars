using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Text;
using PicuCalendars.Security;

namespace TestCalendar
{
    [TestClass]
    public class CreateCryptoKey
    {
        [TestMethod]
        public void GenerateKey()
        {
            var a = CryptoUtilities.GenerateKey(32);
            //Debug.WriteLine("UTF8 - " + new String(Encoding.UTF8.GetChars(a)));
            //Debug.WriteLine("unicode [little Endian] - " + Encoding.Unicode.GetChars(a));
            //Debug.WriteLine("unicode [big endian]- " + Encoding.BigEndianUnicode.GetChars(a));
            //Debug.WriteLine("ASCI - " + Encoding.ASCII.GetChars(a));
            string b64 = Convert.ToBase64String(a);
            Debug.WriteLine("Base64 - " + b64);
            CollectionAssert.AreEqual(a, Convert.FromBase64String(b64));
            Debug.WriteLine("Byte[] - new byte[] {" + string.Join(",", a) + '}');
        }

        [TestMethod]
        public void TestSymetrical()
        {
            const string original = "this is a test string";

            using (var aes = new SimpleAes())
            {
                var enc = aes.Encrypt(original);
                Debug.WriteLine("encrypted = " + enc);
                string decrypted = aes.Decrypt(enc);
                Assert.AreEqual(original, decrypted);

                var secondEnc = aes.Encrypt(original);
                Assert.AreNotEqual(secondEnc, enc);
            }
            
        }
    }
}
