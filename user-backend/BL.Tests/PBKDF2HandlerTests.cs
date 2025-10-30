namespace BL.Tests
{
    using Common.Handlers;
    using Common.Interfaces;
    using Xunit;

    public class PBKDF2HandlerTests
    {
        private readonly IPBKDF2 _IPBKDF2Provider;

        public PBKDF2HandlerTests()
        {
            _IPBKDF2Provider = new PBKDF2Handler();
        }

        #region Check
        [Fact]
        public void Check_ValidInput_ReturnTrue()
        {
            string plainText = "test1";
            string cipherText = "LJ5UCMKiM7+YjOobI1lMdOGF9ND4Rcn/tCZZb8p//S/9T9ov";

            bool actual = _IPBKDF2Provider.Check(cipherText, plainText);

            Assert.True(actual);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("")]
        [InlineData(null)]
        public void Check_InValidInput_ReturnFormatException(string plainText)
        {
            string cipherText = "NotCipherText";

            var actual = Assert.Throws<System.FormatException>(() => _IPBKDF2Provider.Check(cipherText, plainText));
            Assert.Equal("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.", actual.Message);
        }

        [Fact]
        public void Check_InValidCipherText_ReturnTrue()
        {
            string plainText = "test1";
            //string cipherText = _IPBKDF2Provider.Generate("NotTheSamePlainText");
            string cipherText = _IPBKDF2Provider.Generate("ghost123");

            bool actual = _IPBKDF2Provider.Check(cipherText, plainText);

            Assert.False(actual);
        }

        [Fact]
        public void Check_ValidGenerateFun_ReturnTrue()
        {
            string plainText = "test1";
            string cipherText = _IPBKDF2Provider.Generate(plainText);

            bool actual = _IPBKDF2Provider.Check(cipherText, plainText);

            Assert.True(actual);
        }
        #endregion
    }
}
