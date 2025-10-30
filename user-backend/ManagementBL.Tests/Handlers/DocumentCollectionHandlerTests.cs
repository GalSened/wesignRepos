using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static Topshelf.Runtime.Windows.NativeMethods;
using Xunit;
using Common.Enums.Documents;
using Common.Enums.Results;
using Moq;
using Common.Interfaces.DB;
using ManagementBL.Handlers;
using Common.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementBL.Tests.Handlers
{
    public class DocumentCollectionHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";


        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;

        private readonly IDocumentCollection _documentCollectionsHandler;

        public DocumentCollectionHandlerTests()
        {
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _documentCollectionsHandler = new DocumentCollectionHandler(_companyConnectorMock.Object, _documentCollectionConnectorMock.Object);
        }


        public void Dispose()
        {
            _companyConnectorMock.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
        }

        #region ReadByStatusAndDate

        [Fact]
        public async Task ReadByStatusAndDate_CompanyIsNull_ShouldThrowException()
        {
            Company company = null;

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.ReadByStatusAndDate(company, new DateTime(), true));

            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadByStatusAndDate_InvalidDateTime_ShouldThrowException()
        {
            Company company = new Company();
            var datetime = DateTime.Now.AddYears(1);

            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.ReadByStatusAndDate(company, datetime, true));

            Assert.Equal(ResultCode.InvalidDateTime.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadByStatusAndDate_ReturnEmptyDocumentCollectionList_Success()
        {
            Company company = new Company()
            {
                Id = Guid.Parse(GUID),
                CompanyConfiguration = new CompanyConfiguration()
                {
                    DocumentDeletionConfiguration = new Common.Models.Documents.DocumentDeletionConfiguration()
                    {
                        DeleteUnsignedDocumentAfterXDays = 5,
                    }
                }
            };

            var datetime = new DateTime(2022, 1, 1);
            var documentCollections = new List<DocumentCollection>();


            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _documentCollectionConnectorMock.Setup(x => x.ReadByStatusAndDate(It.IsAny<Company>(), It.IsAny<DateTime>())).Returns(documentCollections);

            var documnets = await _documentCollectionsHandler.ReadByStatusAndDate(company, datetime, true);

            Assert.Empty(documnets);
            Assert.Equal(documnets, documentCollections);
        }

        [Fact]
        public async Task ReadByStatusAndDate_ReturnDocumentCollectionList_Success()
        {
            Company company = new Company()
            {
                Id = Guid.Parse(GUID),
                CompanyConfiguration = new CompanyConfiguration()
                {
                    DocumentDeletionConfiguration = new DocumentDeletionConfiguration()
                    {
                        DeleteUnsignedDocumentAfterXDays = 5,
                    }
                }
            };

            var datetime = new DateTime(2023, 1, 2);

            var documentCollections = new List<DocumentCollection>()
            {
                new DocumentCollection()
                {
                    DocumentStatus = DocumentStatus.Created,
                    User = new User()
                    {
                        CompanyId = Guid.Parse(GUID),
                    },
                    CreationTime = new DateTime(2023,1,1)
                },
               new DocumentCollection()
               {
                    DocumentStatus = DocumentStatus.Created,
                    User = new User()
                    {
                        CompanyId = Guid.Parse(GUID),
                    },
                    CreationTime = new DateTime(2023,2,5)
                }
            };

            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _documentCollectionConnectorMock.Setup(x => x.ReadByStatusAndDate(company, It.IsAny<DateTime>()))
                .Returns(documentCollections.FindAll(x =>
                x.User.CompanyId == company.Id &&
                (x.DocumentStatus == DocumentStatus.Viewed || x.DocumentStatus == DocumentStatus.Sent || x.DocumentStatus == DocumentStatus.Created) &&
                x.CreationTime < datetime && datetime.Subtract(x.CreationTime).TotalHours <= 48
                ));

            var documnets = await _documentCollectionsHandler.ReadByStatusAndDate(company, datetime, true);

            Assert.Single( documnets);
        }

        #endregion
    }
}
