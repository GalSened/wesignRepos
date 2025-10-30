namespace Common.Interfaces
{
    public interface IPBKDF2
    {
        /// <summary>
        /// Generate cipher text from plain text
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>cipherText</returns>
        string Generate(string plainText);

        bool Check(string cipherText, string plainText);
    }
}
