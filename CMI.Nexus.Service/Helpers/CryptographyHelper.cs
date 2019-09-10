using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CMI.Nexus.Service
{
    /// <summary>
    /// Helper class for cryptography
    /// </summary>
    public static class CryptographyHelper
    {
        /// <summary>
        /// To encrypt given plain text using given key string
        /// </summary>
        /// <param name="text">Plain text to encrypt</param>
        /// <param name="keyString">Key string to be used for encryption</param>
        /// <returns>Encrypted text</returns>
        public static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        var str = Convert.ToBase64String(result);
                        var fullCipher = Convert.FromBase64String(str);
                        return str;
                    }
                }
            }
        }

        /// <summary>
        /// To decrypt given cipher text
        /// </summary>
        /// <param name="cipherText">Cipher text to decrypt</param>
        /// <param name="keyString">Key string to be used for decryption</param>
        /// <returns>Decrypted plain text</returns>
        public static string DecryptString(string cipherText, string keyString)
        {
            cipherText = cipherText.Replace(" ", "+");
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }
    }
}
