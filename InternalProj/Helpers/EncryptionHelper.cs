using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InternalProj.Helpers
{
    public static class EncryptionHelper
    {
        // AES key and IV — 32-char key (256-bit), 16-char IV (128-bit)
        private static readonly string Key = "HariPriyaSecureKeyForAes12345678"; // Exactly 32 chars
        private static readonly string IV = "InitVector123456";                   // Exactly 16 chars

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return plainText;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }
                var encrypted = Convert.ToBase64String(ms.ToArray());
                Console.WriteLine($"Encrypt: Original='{plainText}', Encrypted='{encrypted}'");
                return encrypted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption failed: {ex.Message}");
                throw;
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                Console.WriteLine("Decrypt: input is empty or null");
                return cipherText;
            }

            if (!IsBase64String(cipherText))
            {
                Console.WriteLine("Decrypt: input is not a valid Base64 string, returning original");
                return cipherText;
            }

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var reader = new StreamReader(cs);

                var decrypted = reader.ReadToEnd();
                Console.WriteLine($"Decrypt: Encrypted='{cipherText}', Decrypted='{decrypted}'");
                return decrypted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption failed: {ex.Message}");
                throw;
            }
        }

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
