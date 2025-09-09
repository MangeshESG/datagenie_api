using System.Security.Cryptography;
using System.Text;

namespace datagenie_api.Services
{
    public class EncryptionService
    {

        /// <summary>
        /// The password hash
        /// </summary>
        static readonly string PasswordHash = "p@@$w0rd";
        /// <summary>
        /// The salt key
        /// </summary>
        static readonly string SaltKey = "D@t@&KEY";
        /// <summary>
        /// The vi key
        /// </summary>
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";


        /// <summary>
        /// Encrypts the specified string input.
        /// </summary>
        /// <param name="strInput">The string input.</param>
        /// <returns>System.String.</returns>
        public static string Encrypt(string strInput)
        {
            string strData = EncryptUsingSalt(strInput);
            strData = EncryptUsingDES(strData);
            return strData;
        }

        /// <summary>
        /// Decrypts the specified string input.
        /// </summary>
        /// <param name="strInput">The string input.</param>
        /// <returns>System.String.</returns>
        public static string Decrypt(string strInput)
        {
            if (strInput.Length > 0)
            {
                string strData = DecryptUsingDES(strInput);
                strData = DecryptUsingSalt(strData);
                return strData;
            }
            else
                return strInput;
        }

        /// <summary>
        /// Encrypts the using salt.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>System.String.</returns>
        private static string EncryptUsingSalt(string plainText, string salt = null)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            if (string.IsNullOrEmpty(salt))
                salt = SaltKey;
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(salt)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>
        /// Encrypts the using DES.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.String.</returns>
        private static string EncryptUsingDES(string data)
        {
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            DES.Key = Encoding.ASCII.GetBytes(VIKey);

            DES.Padding = PaddingMode.PKCS7;
            ICryptoTransform DESEncrypt = DES.CreateEncryptor();
            Byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(data);

            return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
        }

        /// <summary>
        /// Decrypts the using DES.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.String.</returns>
        private static string DecryptUsingDES(string data)
        {
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            DES.Key = Encoding.ASCII.GetBytes(VIKey);

            DES.Padding = PaddingMode.PKCS7;
            ICryptoTransform DESEncrypt = DES.CreateDecryptor();
            Byte[] Buffer = Convert.FromBase64String(data.Replace(" ", "+"));

            return Encoding.UTF8.GetString(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
        }

        /// <summary>
        /// Decrypts the using salt.
        /// </summary>
        /// <param name="encryptedText">The encrypted text.</param>
        /// <returns>System.String.</returns>
        private static string DecryptUsingSalt(string encryptedText, string salt = null)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            if (string.IsNullOrEmpty(salt))
                salt = SaltKey;
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(salt)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
    }
}

