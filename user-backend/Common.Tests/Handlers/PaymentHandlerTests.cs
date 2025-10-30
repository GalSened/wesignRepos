using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ManagementApp;
using Moq;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Common.Tests.Handlers
{
    public class PaymentHandlerTests : IDisposable
    {
        private const string OLD_TRANSACTION_ID = "1";
        private const string NEW_TRANSACTION_ID = "2";
        private const string EXAMPLE_MAIL = "example@mail.co.il";
        
        private readonly Mock<IProgramConnector> _programConnector;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IPrograms> _programs;
        private readonly Mock<Interfaces.ManagementApp.IUsers> _users;
        private readonly Mock<ICompanies> _companies;
        private readonly Mock<IDater> _dater;
        private readonly Mock<ILogger> _logger;
        private readonly IPayment _paymentHandler;

        public PaymentHandlerTests()
        {
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
            _programConnector = new Mock<IProgramConnector>();
            _programs = new Mock<IPrograms>();
            _users = new Mock<Interfaces.ManagementApp.IUsers>();
            _companies = new Mock<ICompanies>();
            _dater = new Mock<IDater>();
            _logger = new Mock<ILogger>();
            
            _paymentHandler = new PaymentHandler(_programConnector.Object, _companyConnectorMock.Object, _programUtilizationConnectorMock.Object, 
                _programs.Object, _users.Object, _companies.Object, _dater.Object, _logger.Object);
        }

        public void Dispose()
        {
            _companyConnectorMock.Invocations.Clear();
            _programUtilizationConnectorMock.Invocations.Clear();            
            _programConnector.Invocations.Clear();
            _programs.Invocations.Clear();
            _users.Invocations.Clear();
            _companies.Invocations.Clear();
            _dater.Invocations.Clear();
            _logger.Invocations.Clear();
        }

        #region UpdateRenwablePayment

        [Fact]
        public async Task UpdateRenwablePayment_NonExistsUser_ThrowInvalidOperationException()
        {
            // Arrange
            UpdatePaymentRenewable updatePaymentRenewable = new UpdatePaymentRenewable();
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync((User)null);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentHandler.UpdateRenwablePayment(updatePaymentRenewable));

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task UpdateRenwablePayment_NonExistsCompany_ThrowInvalidOperationException()
        {
            // Arrange
            UpdatePaymentRenewable updatePaymentRenewable = new UpdatePaymentRenewable();
            User dbUser = new User();
            User user = null;
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(dbUser).Callback<User>(_ => user = _);
            _companies.Setup(_ => _.Read(It.IsAny<Company>(), It.IsAny<User>())).ReturnsAsync((CompanyExpandedDetails)null);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentHandler.UpdateRenwablePayment(updatePaymentRenewable));

            // Assert
            _users.Verify(_ => _.Read(It.IsAny<User>()), Times.Once);
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
            Assert.NotNull(user);
        }

        [Fact]
        public async Task UpdateRenwablePayment_InvalidTransactionId_ThrowInvalidOperationException()
        {
            // Arrange
            UpdatePaymentRenewable updatePaymentRenewable = new UpdatePaymentRenewable()
            {
                OldTransactionId = OLD_TRANSACTION_ID
            };
            User user = new User();
            CompanyExpandedDetails company = new CompanyExpandedDetails()
            {
                TransactionId = NEW_TRANSACTION_ID,
            };
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companies.Setup(_ => _.Read(It.IsAny<Company>(), It.IsAny<User>())).ReturnsAsync(company);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentHandler.UpdateRenwablePayment(updatePaymentRenewable));

            // Assert
            Assert.Equal(ResultCode.InvalidTransactionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task UpdateRenwablePayment_UpdateTransactionId_ShouldSucceeded()
        {
            // Arrange
            UpdatePaymentRenewable updatePaymentRenewable = new UpdatePaymentRenewable()
            {
                OldTransactionId = OLD_TRANSACTION_ID
            };
            User user = new User();
            CompanyExpandedDetails dbCompany = new CompanyExpandedDetails()
            {
                TransactionId = OLD_TRANSACTION_ID
            };
            Company company = new Company();
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companies.Setup(_ => _.Read(It.IsAny<Company>(), It.IsAny<User>())).ReturnsAsync(dbCompany);

            // Action
            await _paymentHandler.UpdateRenwablePayment(updatePaymentRenewable);

            // Assert
            _companies.Verify(_ => _.Read(It.IsAny<Company>(), It.IsAny<User>()), Times.Once);
            _companies.Verify(_ => _.UpdateTransactionId(It.IsAny<Company>()), Times.Once);
        }

        #endregion

        #region UserPay

        [Fact]
        public async Task UserPay_NonExistsUser_ThrowInvalidOperationException()
        {
            // Arrange
            UserPayment userPayment = new UserPayment();
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync((User)null);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentHandler.UserPay(userPayment));

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task UserPay_NonExistsProgram_ThrowInvalidOperationException()
        {
            // Arrange
            UserPayment userPayment = new UserPayment();
            User dbUser = new User();
            User user = null;
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(dbUser).Callback<User>(_ => user = _);
            _programs.Setup(_ => _.Read(It.IsAny<Program>())).ReturnsAsync((Program)null);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentHandler.UserPay(userPayment));

            // Assert
            _users.Verify(_ => _.Read(It.IsAny<User>()), Times.Once);
            Assert.Equal(ResultCode.ProgramNotExist.GetNumericString(), actual.Message);
            Assert.NotNull(user);
        }

        [Fact]
        public async Task UserPay_FreeTrialUser_ShouldSuccess()
        {
            // Arrange
            UserPayment userPayment = new UserPayment();
            User user = new User()
            {
                Email = EXAMPLE_MAIL
            };
            Program dbProgram = new Program();
            Program program = null;
            _users.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            _programs.Setup(_ => _.Read(It.IsAny<Program>())).ReturnsAsync(dbProgram).Callback<Program>(_ => program = _);
            _programConnector.Setup(_ => _.IsFreeTrialUser(It.IsAny<User>())).Returns(true);

            // Action
            await _paymentHandler.UserPay(userPayment);

            // Assert
            _companies.Verify(_ => _.Create(It.IsAny<Company>(), It.IsAny<Group>(), It.IsAny<User>()), Times.Once);
            _companies.Verify(_ => _.ResendResetPasswordMail(It.IsAny<User>()), Times.Once);
            Assert.NotNull(program);
        }

        #endregion
    }
}