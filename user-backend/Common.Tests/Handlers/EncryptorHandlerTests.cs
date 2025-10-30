using Common.Handlers;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Common.Tests.Handlers
{
    public class EncryptorHandlerTests
    {

        private readonly IEncryptor _encryptor;

        public EncryptorHandlerTests()
        {
            _encryptor = new EncryptorHandler();
        }


        #region Decrypt

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Decrypt_InvalidInput_Failed(string input)
        {
            string actual = _encryptor.Decrypt(input);

            Assert.Equal("", actual);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("abc 123 @!%#@#^^")]
        [InlineData("213202482408420")]
        [InlineData("dndjndjndjndjndj")]
        [InlineData("2251bf45-b92b-4aac-9606-cf82b0ce35d3")]
        [InlineData("dndjndjndjndjndj  [InlineData(\"dndjndjndjndjndj\")]123")]

        [InlineData("2251bf45-b92b-4aac-9606-cf82b0ce35d3_2251bf45-b92b-4aac-9606-cf82b0ce35d3_2251bf45-b92b-4aac-9606-cf82b0ce35d3_2251bf45-b92b-4aac-9606-cf82b0ce35d3")]
        //[InlineData("vAPSeB8J2ODa9JwTOU8+/vuYFhBGm73cO5W9YxM8JTa3gwCEkPt9WvUNWgsKhjB2fv78nU4ofsF7cnOpF3nrAVJNh5RWCWEH3KvTI9FonCA=")]
        // UP TO 16 CHARS
        public void Decrpt_CheckAlgorithmCorrectness_Success(string text)
        {
            string cipherText = _encryptor.Encrypt(text);

            string plainText = _encryptor.Decrypt(cipherText);

            Assert.Equal(text, plainText);
        }

        #endregion

        #region Encrypt

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Encrypt_InvalidInput_Failed(string input)
        {
            string actual = _encryptor.Encrypt(input);

            Assert.Equal("", actual);
        }

        #endregion


    }
}
