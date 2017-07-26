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
    }
}