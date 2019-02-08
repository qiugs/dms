using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace RMA_Docker.Classes {
    public class Algorithm {
        public String GetCipherKey() { return "JR=7n_"; }

        public String EncryptString(String inputString) {
            return (new Cipher().SHA256Encrypt(inputString, GetCipherKey()));
        }

        public String DecryptString(String encryptedString) {
            return (new Cipher().SHA256Decrypt(encryptedString, GetCipherKey()));
        }

        private class Cipher {
            public String SHA256Encrypt(String val, String secretKey) {
                if (val == null) { return null; }
                if (secretKey == null) { secretKey = String.Empty; }
                var bytesToBeEncrypt = Encoding.UTF8.GetBytes(val);
                var screctKeyBytes = Encoding.UTF8.GetBytes(secretKey);

                screctKeyBytes = SHA256.Create().ComputeHash(screctKeyBytes);
                var bytesEncrypted = Cipher.Encrypt(bytesToBeEncrypt, screctKeyBytes);
                return Convert.ToBase64String(bytesEncrypted);
            }

            public String SHA256Decrypt(String ecryptVal, String secretKey) {
                if (ecryptVal == null) { return null; }
                if (secretKey == null) { secretKey = String.Empty; }
                var bytesToBeDecrypted = Convert.FromBase64String(ecryptVal);
                var screctKeyBytes = Encoding.UTF8.GetBytes(secretKey);

                screctKeyBytes = SHA256.Create().ComputeHash(screctKeyBytes);
                var bytesDecrypted = Cipher.Decrypt(bytesToBeDecrypted, screctKeyBytes);
                return Encoding.UTF8.GetString(bytesDecrypted);
            }

            private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes) {
                byte[] encryptedBytes = null;
                var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream()) {
                    using (RijndaelManaged AES = new RijndaelManaged()) {
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);
                        AES.Mode = CipherMode.CBC;
                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write)) {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }
                return encryptedBytes;
            }

            private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes) {
                byte[] decryptedBytes = null;
                var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream()) {
                    using (RijndaelManaged AES = new RijndaelManaged()) {
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);
                        AES.Mode = CipherMode.CBC;
                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write)) {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }
                return decryptedBytes;
            }
        }
    } 
}