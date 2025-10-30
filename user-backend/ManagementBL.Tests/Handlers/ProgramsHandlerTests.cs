using Common.Enums.Companies;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using ManagementBL.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class ProgramsHandlerTests
    {
        private readonly Guid ID = new Guid("00000000-0000-0000-0000-000000000004");
        private readonly Guid CALLBACK_ID = new Guid("00000000-0000-0000-0000-000000000008");
        private readonly IPrograms _program;
        private readonly Mock<IProgramConnector> _programConnecktorMock;
        private readonly Mock<ICompanyConnector> _companyConnecktorMock;


        public ProgramsHandlerTests()
        {
            _companyConnecktorMock = new Mock<ICompanyConnector>();
            _programConnecktorMock = new Mock<IProgramConnector>();
            _program = new ProgramsHandler(_programConnecktorMock.Object, _companyConnecktorMock.Object);
        }


        #region Create

        [Fact]
        public async Task Create_NullInput_ThrowException()
        {
            Program program = null;
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _program.Create(program));

            Assert.Equal(ResultCode.ProgramAlreadyExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_ValidProgram_Success()
        {
            Program program = new Program();
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(false);
            _programConnecktorMock.Setup(x => x.Create(program)).Callback(() => program.Id = ID);

            await _program.Create(program);

            Assert.Equal(program.Id, ID);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_NullInput_ThrowException()
        {
            Program program = null;
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(false);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _program.Delete(program));

            Assert.Equal(ResultCode.ProgramNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_ProgramWithRelatedCompanies_ThrowException()
        {
            var program = new Program { Id = ID };           
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _companyConnecktorMock.Setup(x => x.ReadCompaniesByProgram(It.IsAny<Program>()))
                .Returns(new List<Company> { new Company { Id = Guid.NewGuid(), ProgramId = ID } });

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _program.Delete(program));

            Assert.Equal(ResultCode.ThereAreRelatedCompaniesToThisProgram.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_ProgramWithoutRelatedCompanies_Success()
        {
            var program = new Program { Id = ID };
            
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _companyConnecktorMock.Setup(x => x.ReadCompaniesByProgram(It.IsAny<Program>()))
                .Returns(new List<Company>());
            _programConnecktorMock.Setup(x => x.Delete(program)).Callback(() => program.Id = CALLBACK_ID);

            await _program.Delete(program);

            Assert.Equal(program.Id, CALLBACK_ID);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_NullInput_ThrowException()
        {
            Program program = null;
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(false);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _program.Update(program));

            Assert.Equal(ResultCode.ProgramNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_ValidInput_ThrowException()
        {
            Program program = new Program();
            _programConnecktorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _programConnecktorMock.Setup(x => x.Update(It.IsAny<Program>())).Callback(() => { program.Id = CALLBACK_ID; });

            await _program.Update(program);

            Assert.Equal(program.Id, CALLBACK_ID);
        }

        #endregion

    }
}
