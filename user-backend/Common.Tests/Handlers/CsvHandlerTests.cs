using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Handlers;
using System;
using System.Collections.Generic;
using Xunit;

namespace Common.Tests.Handlers
{
    public class CsvHandlerTests
    {
        #region ExportDocumentCollection

        [Fact]
        public void ExportDocumentCollection_Empty_ThrowInvalidOperationException()
        {
            // Arrange
            IEnumerable<object> DTOCollection = new List<object>();

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => CsvHandler.ExportDocumentsCollection(DTOCollection, Language.en));

            // Assert
            Assert.Equal(ResultCode.InvalidObjectType.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
