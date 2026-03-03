using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceApp.Helpers
{
    public static class CryptoHelper
    {
        // store this securely (not hardcoded)
        private static readonly byte[] Key =
            Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes

        private static readonly byte[] IV =
            Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                var encryptor = aes.CreateEncryptor();

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                    sw.Close();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                var decryptor = aes.CreateDecryptor();

                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}