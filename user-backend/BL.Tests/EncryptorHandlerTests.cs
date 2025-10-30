
using Common.Handlers;
using Common.Interfaces;
using Xunit;

namespace BL.Tests
{
    public class EncryptorHandlerTests
    {
        private IEncryptor _encryptor;
        public EncryptorHandlerTests()
        {
            _encryptor = new EncryptorHandler();
        }

        [Fact]
        public void test()
        {
            //283
            string pass = "ghost123";
            var cipher = _encryptor.Encrypt(pass);
            var jd = _encryptor.Decrypt(cipher);
            string jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zaWQiOiJkMjIxYjEyMy1iZDkxLTRjMmEtOWEwMS0wOGQ3NjFkMTdiNmQiLCJuYmYiOjE1NzI5NDU2NjUsImV4cCI6MTU3NTUzNzY2NSwiaWF0IjoxNTcyOTQ1NjY1fQ.Rp9tLJZPq50k3mVrIKLja3-lCm4AfVBY2X5JvkAi7Ks";
            //var cipher = _encryptor.Encrypt(jwt);
            var ciphesr = _encryptor.Encrypt("sdgbnsdkjbsdgk");
            var length = cipher.Length;
            var response = _encryptor.Decrypt(cipher);
            var response2 = _encryptor.Decrypt(ciphesr);
            bool isEqual = response == jwt;
        }
    }
}
