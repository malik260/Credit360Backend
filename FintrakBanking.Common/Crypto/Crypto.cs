using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Crypto
{
    public static class Crypto
    {
        // encryption function similar to Angular cryptoJs
        public static string NgEncrypt(string plainText, string key, string salt)
        {
            //var cipherTextArray = ngCipherKeyIvPhrase.Split("|");
            //var cipherTextArray = ngCipherKeyIvPhrase.Split(new Char[] { '|' });
            //string cipherPhrase = cipherTextArray[0];
            //string salt = cipherTextArray[1];
            //private const string salt128 = "kljsdkkdlo4454GG";
            //private const string salt256 = "kljsdkkdlo4454GG00155sajuklmbkdl";

            byte[] encrypted;
            // Create a RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                //rijAlg.Key = Encoding.UTF8.GetBytes(cipherPhrase);
                //rijAlg.IV = Encoding.UTF8.GetBytes(salt);
                //rijAlg.BlockSize = 
                rijAlg.Key = Encoding.UTF8.GetBytes(key);
                rijAlg.IV = Encoding.UTF8.GetBytes(salt);

                // Create a decrytor to perform the stream transform.  
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.  
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.  
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.  
            return Convert.ToBase64String(encrypted);
        }

        // decryption function similar to Angular cryptoJs
        public static string NgDecrypt(string encryptedText, string ngCipherKeyIvPhrase)
        {
            string plainText = string.Empty;
            var cipherTextArray = ngCipherKeyIvPhrase.Split(new Char[] { '|' });
            string cipherPhrase = cipherTextArray[0];
            string salt = cipherTextArray[1];

            byte[] cipherText = Convert.FromBase64String(encryptedText);
            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = Encoding.UTF8.GetBytes(cipherPhrase);
                rijAlg.IV = Encoding.UTF8.GetBytes(salt);

                // Create a decryptor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.  
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {

                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream  
                            // and place them in a string.  
                            plainText = srDecrypt.ReadToEnd();

                        }

                    }
                }

                return plainText;

            }

        }
    }
}
