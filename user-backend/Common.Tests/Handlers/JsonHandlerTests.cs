using Common.Handlers;
using Common.Interfaces;
using Common.Models.Documents;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Common.Tests.Handlers
{
    public class JsonHandlerTests
    {
        private readonly IJson _json;

        public JsonHandlerTests()
        {
            _json = new JsonHandler();
        }

        #region Serialize

        [Fact]
        public void Serialize_NullInput_ReturnEmptyString()
        {
            string jsonFormat = _json.Serialize(null);

            Assert.Equal(string.Empty, jsonFormat);
        }

        [Fact]
        public void Serialize_StringInput_ReturnStringWithQuotationMarks()
        {
            string str = "Non object";
            string jsonFormat = _json.Serialize(str);

            Assert.Equal($"\"{str}\"", jsonFormat);
        }

        [Fact]
        public void Serialize_ObjectInput_Success()
        {
            Appendix appendix = new Appendix { Name = "appendixName", Base64File="file content" };
            string excepted = "{\"Name\":\"appendixName\",\"Base64File\":\"file content\",\"FileExtention\":null,\"FileContent\":null}";
            string jsonFormat = _json.Serialize(appendix);

            Assert.Equal(excepted, jsonFormat);
        }

        #endregion

        #region Desrialize

        [Fact]
        public void Desrialize_NullInput_ThrowException()
        {
            var actual =Assert.Throws<ArgumentNullException>(()=> _json.Desrialize<Appendix>(null));

            Assert.True(actual is ArgumentNullException);
        }

        [Fact]
        public void Desrialize_ValidInput_Success()
        {
            Appendix appendix = new Appendix { Name = "appendixName", Base64File = "file content" };
            string jsonFormat = _json.Serialize(appendix);
            Appendix actual =  _json.Desrialize<Appendix>(jsonFormat);

            Assert.Equal(appendix.Name, actual.Name);
            Assert.Equal(appendix.Base64File, actual.Base64File);
        }

        #endregion
    }
}
