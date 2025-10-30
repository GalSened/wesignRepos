using Common.Consts;
using Common.Enums.Companies;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class ProgramsHandler : IPrograms
    {
        private readonly IProgramConnector _programConnector;
        private readonly ICompanyConnector _companyConnector;

        public ProgramsHandler(IProgramConnector  programConnector, ICompanyConnector companyConnector)
        {
            _programConnector = programConnector;
            _companyConnector = companyConnector;
        }

        public async Task Create(Program program)
        {
            bool isExist =await _programConnector.Exists(program);
            if (isExist)
            {
                throw new InvalidOperationException(ResultCode.ProgramAlreadyExist.GetNumericString());
            }
            await _programConnector.Create(program);
        }

        public async Task Delete(Program program)
        {
            bool isExist =await _programConnector.Exists(program);
            if (!isExist)
            {
                throw new InvalidOperationException(ResultCode.ProgramNotExist.GetNumericString());
            }
            IEnumerable<Company> companies = _companyConnector.ReadCompaniesByProgram(program);
            
            if (companies?.Any() ?? false)
            {
                throw new InvalidOperationException(ResultCode.ThereAreRelatedCompaniesToThisProgram.GetNumericString());
            }
            await _programConnector.Delete(program);
        }

        public IEnumerable<Program> Read(string key, int offset, int limit, out int totalCount)
        {
            var programs = _programConnector.Read(key, offset, limit, out totalCount);

            return programs;
        }

        public async Task<Program> Read(Program program)
        {
            program = await _programConnector.Read(program);

            return program;
        }

        public async Task Update(Program program)
        {
            var isExist =await _programConnector.Exists(program);
            if (!isExist)
            {
                throw new InvalidOperationException(ResultCode.ProgramNotExist.GetNumericString());
            }
            await _programConnector.Update(program);

        }
    }
}
