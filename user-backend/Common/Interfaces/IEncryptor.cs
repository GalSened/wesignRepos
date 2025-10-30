
namespace Common.Interfaces
{
    public interface IEncryptor
    {
        string Decrypt(string cipherText);

        string Encrypt(string plainText);
    }
}
