using System;
using System.Security.Cryptography;
using System.Text;

namespace FintrakBanking.Common
{
    public static class StaticHelpers
    {
        public static string EncryptionKey
        {
            get { return "Login_Salt@FintrakBanking"; }
        }

        public static string EncryptSha512(this string value, string salt)
        {
            return Encrypt(value, salt, SHA512.Create());
        }

        private static string Encrypt(string value, string salt, HashAlgorithm hashAlgorithm)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var toHash = new byte[valueBytes.Length + saltBytes.Length];

            Array.Copy(valueBytes, 0, toHash, 0, valueBytes.Length);
            Array.Copy(saltBytes, 0, toHash, valueBytes.Length, saltBytes.Length);

            var hash = hashAlgorithm.ComputeHash(toHash);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}