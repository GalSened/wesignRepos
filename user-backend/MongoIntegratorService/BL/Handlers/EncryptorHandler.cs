using HistoryIntegratorService.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace HistoryIntegratorService.BL.Handlers
{
    public class EncryptorHandler : IEncryptor
    {
        private const int KEY_SIZE = 128;
        // This constant determines the number of iterations for the password bytes generation function.
        private const int DERIVATION_ITERATIONS = 1000;
        private const string PASSWORD = "pass";
        private readonly int _keySizeInBytes;

        public EncryptorHandler()
        {
            _keySizeInBytes = KEY_SIZE / 8;
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cipherText))
                {
                    return "";
                }
                // Get the complete stream of bytes that represent:
                // [16 bytes of Salt] + [16 bytes of IV] + [n bytes of CipherText]
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                //var cipherTextBytesWithSaltAndIv = Encoding.UTF8.GetBytes(cipherText);
                // Get the saltbytes by extracting the first 16 bytes from the supplied cipherText bytes.
                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(_keySizeInBytes)
                                                                  .ToArray();
                // Get the IV bytes by extracting the next 16 bytes from the supplied cipherText bytes.
                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(_keySizeInBytes)
                                                                .Take(_keySizeInBytes)
                                                                .ToArray();
                // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(_keySizeInBytes * 2)
                                                                  .Take(cipherTextBytesWithSaltAndIv.Length - (_keySizeInBytes * 2))
                                                                  .ToArray();

                using (var password = new Rfc2898DeriveBytes(PASSWORD, saltStringBytes, DERIVATION_ITERATIONS))
                {
                    var keyBytes = password.GetBytes(_keySizeInBytes);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = KEY_SIZE;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];

                                    int decryptedByteCount = 0;
                                    while (decryptedByteCount < plainTextBytes.Length)
                                    {
                                        int bytesRead = cryptoStream.Read(plainTextBytes, decryptedByteCount, plainTextBytes.Length - decryptedByteCount);

                                        if (bytesRead == 0) break;
                                        decryptedByteCount += bytesRead;
                                    }

                                    //var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string Encrypt(string plainText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plainText))
                {
                    return "";
                }
                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
                // so that the same Salt and IV values can be used when decrypting.  
                var saltStringBytes = Generate128BitsOfRandomEntropy();
                var ivStringBytes = Generate128BitsOfRandomEntropy();
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (var password = new Rfc2898DeriveBytes(PASSWORD, saltStringBytes, DERIVATION_ITERATIONS))
                {
                    var keyBytes = password.GetBytes(_keySizeInBytes);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = KEY_SIZE;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                    var cipherTextBytes = saltStringBytes;
                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Convert.ToBase64String(cipherTextBytes);
                                    //return Encoding.UTF8.GetString(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        private byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
