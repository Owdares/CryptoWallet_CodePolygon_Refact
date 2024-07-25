using System.Security.Cryptography;

namespace Blaved.Core.Utility
{
    public class Cryptography
    {
        public static string Encrypt(string value, string base64Key, string base64IV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyBytes = Convert.FromBase64String(base64Key);
                byte[] ivBytes = Convert.FromBase64String(base64IV);

                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(valueBytes, 0, valueBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public static string Decrypt(string value, string base64Key, string base64IV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyBytes = Convert.FromBase64String(base64Key);
                byte[] ivBytes = Convert.FromBase64String(base64IV);

                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] encryptedBytes = Convert.FromBase64String(value);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return System.Text.Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }

}
