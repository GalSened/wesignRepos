using Common.Enums.PDF;
using Common.Enums.Reports;
using Common.Extensions;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ManagementApp.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using WeSignManagement.Models.Companies;
using WeSignManagement.Models.Companies.Responses;
using WeSignManagement.Models.Programs;
using WeSignManagement.Models.Reports;
using WeSignManagement.Request;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/reports")]
#else
    [Route("v3/reports")]
#endif
    [Authorize(Roles = "SystemAdmin")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]

    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ReportsController : ControllerBase
    {
        private readonly IManagementBL _bl;

        public ReportsController(IManagementBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllProgramUtilizationHistoriesResponseDTO))]
        public IActionResult Read(string key = null, int offset = 0, int limit = 20, DateTime? from = null, DateTime? to = null)
        {
            var data = _bl.Reports.Read(key, offset, limit, from, to, out int totalCount);
            Response.Headers.Add("x-total-count", totalCount.ToString());

            return Ok(new AllProgramUtilizationHistoriesResponseDTO { Reports = data });
        }


        [HttpGet]
        [Route("UtilizationReport/Expired")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllCompaniesReportsDTO))]
        public async Task<IActionResult> Read(bool? isExpired = null, int monthsForAvgUse = 3, Guid? programId = null, DateTime? from = null, DateTime? to = null, int offset = 0, int limit = 20, bool isCSV = false)
        {
            (var companyReports, int totalCount) = await _bl.Reports.GetUtilizationReports(isExpired, monthsForAvgUse, programId, from, to, offset, limit);

            if (!isCSV)
            {
                List<CompanyReportDTO> response = new List<CompanyReportDTO>();
                foreach (var companyReport in companyReports)
                {
                    response.Add(new CompanyReportDTO(companyReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());

                return Ok(new AllCompaniesReportsDTO { companyReports = response });
            }
            else
            {
                return ExportCSV(companyReports, "Utilization");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllCompaniesReportsDTO))]
        [Route("UtilizationReport/Program/{programID}")]
        public async Task<IActionResult> Read(Guid programID, int monthsForAvgUse = 3, int minDocs = 0, int minSMS = 0, int offset = 0, int limit = 20, bool isCSV = false)
        {
            (var companyReports, int totalCount) = await _bl.Reports.GetUtilizationReportsByProgram(programID, monthsForAvgUse, minDocs, minSMS, offset, limit);

            if (!isCSV)
            {
                List<CompanyReportDTO> response = new List<CompanyReportDTO>();
                foreach (var companyReport in companyReports)
                {
                    response.Add(new CompanyReportDTO(companyReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());

                return Ok(new AllCompaniesReportsDTO { companyReports = response });
            }
            else
            {
                return ExportCSV(companyReports, "UtilizationReportsByProgram");
            }

        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllCompaniesReportsDTO))]
        [Route("UtilizationReport/Percentage")]
        public IActionResult Read(int docsUsagePercentage = 0, int monthsOfUse = 3, int offset = 0, int limit = 20, bool isCSV = false)
        {
            var companyReports = _bl.Reports.GetUtilizationReportsByPercentage(docsUsagePercentage, monthsOfUse, offset, limit, out int totalCount);

            if (!isCSV)
            {
                List<CompanyReportDTO> response = new List<CompanyReportDTO>();
                foreach (var companyReport in companyReports)
                {
                    response.Add(new CompanyReportDTO(companyReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());

                return Ok(new AllCompaniesReportsDTO { companyReports = response });
            }
            else
            {
                return ExportCSV(companyReports, "UtilizationReportsByPercentage");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllGroupReportsDTO))]
        [Route("UtilizationReport/GroupUtilization/{companyID}")]
        public async Task<IActionResult> ReadGroup(Guid companyId, int docsUsagePercentage = 0, int monthsOfUse = 3, int offset = 0, int limit = 20, bool isCSV = false)
        {
            (IEnumerable<GroupUtilizationReport> groupReports, int totalCount) = await _bl.Reports.GetUtilizationReportPerGroup(companyId, docsUsagePercentage, monthsOfUse, offset, limit);

            if (!isCSV)
            {
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllGroupReportsDTO { groupReports = groupReports });
            }
            else
            {
                return ExportCSV(groupReports, "UtilizationReportPerGroup");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllCompaniesReportsDTO))]
        [Route("UtilizationReport/AllCompanies")]
        public IActionResult ReadAllCompanies(int monthsForAvgUse = 3, int offset = 0, int limit = 20, bool isCSV = false)
        {
            var companyReports = _bl.Reports.GetAllCompaniesUtilizations(monthsForAvgUse, offset, limit, out int totalCount);

            if (!isCSV)
            {
                List<CompanyReportDTO> response = new List<CompanyReportDTO>();

                foreach (var companyReport in companyReports)
                {
                    response.Add(new CompanyReportDTO(companyReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());

                return Ok(new AllCompaniesReportsDTO { companyReports = response });
            }
            else
            {
                return ExportCSV(companyReports, "AllCompaniesUtilizations");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllProgramsResponseDTO))]
        [Route("Programs")]
        public IActionResult GetPrograms(int minDocs = 0, int minSMS = 0, int offset = 0, int limit = 20, bool isCSV = false)
        {

            IEnumerable<Common.Models.Program> programs = _bl.Reports.GetProgramsReport(minDocs, minSMS, null, offset, limit, out int totalCount);

            if (!isCSV)
            {
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllProgramsResponseDTO { Programs = programs.ToList() });
            }
            else
            {
                return ExportCSV(programs, "Programs");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllProgramsResponseDTO))]
        [Route("UnusedPrograms")]
        public IActionResult GetUnusedPrograms(bool? isUsed = null, int offset = 0, int limit = 20, bool isCSV = false)
        {
            var programs = _bl.Reports.GetProgramsReport(0, 0, isUsed, offset, limit, out int totalCount);

            if (!isCSV)
            {
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllProgramsResponseDTO { Programs = programs.ToList() });
            }
            else
            {
                return ExportCSV(programs, "UnusedPrograms");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllGroupDocumentReportsDTO))]
        [Route("GroupDocumentReports/{companyID}")]
        public async Task<IActionResult> ReadCompanyDocsStatus(Guid CompanyID, int offset = 0, int limit = 20, bool isCSV = false)
        {
            (IEnumerable<GroupDocumentReport> companyGroupsDocs, int totalCount) = await _bl.Reports.GetGroupDocumentReports(CompanyID, offset, limit);
            if (!isCSV)
            {
                var data = new List<GroupDocumentReportDTO>();
                foreach (var companyGroupDoc in companyGroupsDocs)
                {
                    data.Add(new GroupDocumentReportDTO(companyGroupDoc));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllGroupDocumentReportsDTO { groupDocumentReports = data });
            }
            else
            {
                return ExportCSV(companyGroupsDocs, "CompanyDocsStatus");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUserDocumentReportsDTO))]
        [Route("DocsByUsers/{companyId}")]
        public async Task<IActionResult> GetDocsByGroupUsers(Guid companyId, List<Guid> groupIds = null, DateTime? from = null, DateTime? to = null, int offset = 0, int limit = 20, bool isCSV = false)
        {
            (var docReports, int totalCount) = await _bl.Reports.GetUserDocumentReports(companyId, groupIds, true, from, to, offset, limit);
            if (!isCSV)
            {
                var data = new List<UserDocumentReportDTO>();
                foreach (var userDocReport in docReports)
                {
                    data.Add(new UserDocumentReportDTO(userDocReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllUserDocumentReportsDTO { userDocumentReports = data });
            }
            else
            {
                return ExportCSV(docReports, "DocsByGroupUsers");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUserDocumentReportsDTO))]
        [Route("DocsBySigners/{companyId}")]
        public async Task<IActionResult> GetDocsByGroupSigners(Guid companyId, List<Guid> groupIds = null, DateTime? from = null, DateTime? to = null, int offset = 0, int limit = 20, bool isCSV = false)
        {
            (var docReports, int totalCount) = await _bl.Reports.GetUserDocumentReports(companyId, groupIds, false, from, to, offset, limit);
            if (!isCSV)
            {
                var data = new List<UserDocumentReportDTO>();
                foreach (var userDocReport in docReports)
                {
                    data.Add(new UserDocumentReportDTO(userDocReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllUserDocumentReportsDTO { userDocumentReports = data });
            }
            else
            {
                return ExportCSV(docReports, "DocsByGroupSigners");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllCompanyUsersReportDTO))]
        [Route("UsersByCompany/{companyId}")]
        public async Task<IActionResult> GetUsersByCompany(Guid companyId, int offset = 0, int limit = 20, bool isCSV = false)
        {

            (var companyUsersReports, int totalCount) = await _bl.Reports.GetUsersByCompany(companyId, offset, limit);
            if (!isCSV)
            {
                var data = new List<CompanyUserReportDTO>();
                foreach (var companyUserReport in companyUsersReports)
                {
                    data.Add(new CompanyUserReportDTO(companyUserReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllCompanyUsersReportDTO { companyUsersReports = data });
            }
            else
            {
                return ExportCSV(companyUsersReports, "UsersByCompany");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllFreeTrialUsersReportsDTO))]
        [Route("FreeTrialUsers")]
        public IActionResult GetFreeTrialUsers(int offset = 0, int limit = 20, bool isCSV = false)
        {
            var freeTrialUsersReports = _bl.Reports.GetFreeTrialUsers(offset, limit, out int totalCount);
            if (!isCSV)
            {
                var data = new List<FreeTrialUsersReportDTO>();
                foreach (var freeTrialUsersReport in freeTrialUsersReports)
                {
                    data.Add(new FreeTrialUsersReportDTO(freeTrialUsersReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllFreeTrialUsersReportsDTO { FreeTrialUsersReports = data });
            }
            else
            {
                return ExportCSV(freeTrialUsersReports, "FreeTrialUsers");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUsageByUserReportsDTO))]
        [Route("UsageByUsers")]
        public async Task<IActionResult> GetUsageByUsers(Guid companyId = default, string userEmail = null, List<Guid> groupIds = null, int offset = 0, int limit = int.MaxValue, DateTime? from = null, DateTime? to = null, bool isCSV = false)
        {
            (IEnumerable<UsageByUserReport> usageByUsersReports, int totalCount) = await _bl.Reports.GetUsageByUsers(userEmail, companyId, groupIds, from, to, offset, limit);
            if (!isCSV)
            {
                var data = new List<UsageByUserReportDTO>();
                foreach (var usageByUsersReport in usageByUsersReports)
                {
                    data.Add(new UsageByUserReportDTO(usageByUsersReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllUsageByUserReportsDTO { UsageByUsersReports = data });
            }
            else
            {
                return ExportCSV(usageByUsersReports, "UsageByUsers");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUsageByCompanyReportsDTO))]
        [Route("UsageByCompanies/{companyId}")]
        public async Task<IActionResult> GetUsageByCompanies(Guid companyId, List<Guid> groupIds = null, int offset = 0, int limit = int.MaxValue, DateTime? from = null, DateTime? to = null, bool isCSV = false)
        {
            (var usageByCompaniesReports, int totalCount) = await _bl.Reports.GetUsageByCompanies(companyId, groupIds, from, to, offset, limit);
            if (!isCSV)
            {
                var data = new List<UsageByCompanyReportDTO>();
                foreach (var usageByCompanyReport in usageByCompaniesReports)
                {
                    data.Add(new UsageByCompanyReportDTO(usageByCompanyReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllUsageByCompanyReportsDTO { UsageByCompaniesReports = data });
            }
            else
            {
                return ExportCSV(usageByCompaniesReports, "UsageByCompanies");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllTemplatesByUsageReportDTO))]
        [Route("TemplatesByUsage/{companyId}")]
        public async Task<IActionResult> GetTemplatesByUsage(Guid companyId, List<Guid> groupIds = null, int offset = 0, int limit = int.MaxValue, DateTime? from = null, DateTime? to = null, bool isCSV = false)
        {
            (var templatesByUsageReports, int totalCount) = await _bl.Reports.GetTemplatesByUsage(companyId, groupIds, from, to, offset, limit);
            if (!isCSV)
            {
                var data = new List<TemplatesByUsageReportDTO>();
                foreach (var templatesByUsageReport in templatesByUsageReports)
                {
                    data.Add(new TemplatesByUsageReportDTO(templatesByUsageReport));
                }
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(new AllTemplatesByUsageReportDTO { TemplatesByUsageReports = data });
            }
            else
            {
                return ExportCSV(templatesByUsageReports, "TemplatesByUsage");
            }
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UsageBySignatureTypeReportDTO))]
        [Route("UsageBySignatureType/{companyId}")]
        public async Task<IActionResult> GetUsageBySignatureType(Guid companyId, List<SignatureFieldType> signatureTypes = null, DateTime? from = null, DateTime? to = null, bool isCSV = false)
        {
            var usageBySignatureTypesReport = await _bl.Reports.GetUsageBySignatureType(companyId, signatureTypes, from, to);
            if (!isCSV)
            {
                UsageBySignatureTypeReportDTO data = null;
                if (usageBySignatureTypesReport != null)
                {
                    data = new UsageBySignatureTypeReportDTO(usageBySignatureTypesReport);
                }
                return Ok(data);
            }
            else
            {
                return ExportCSV(new List<UsageBySignatureTypeReport>() { usageBySignatureTypesReport }, "UsageBySignatureType");
            }
        }

        [HttpPost]
        [Route("FrequencyReport")]
        public async Task<IActionResult> CreateFrequencyReport([FromBody] FrequencyReportRequest request)
        {
            await _bl.Reports.CreateFrequencyReport(request.ReportParameters, request.Frequency, request.ReportType, request.EmailsToSend);
            return Ok();
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllManagementPeriodicReportsDTO))]
        [Route("FrequencyReports")]
        public async Task<IActionResult> ReadFrequencyReports()
        {
            var frequencyReports = await _bl.Reports.ReadFrequencyReports();
            if (frequencyReports != null)
            {
                AllManagementPeriodicReportsDTO data = new AllManagementPeriodicReportsDTO() { managementPeriodicReports = frequencyReports };
                return Ok(data);
            }
            return NoContent();
        }

        [HttpPut]
        [Route("FrequencyReports")]
        public async Task<IActionResult> UpdateFrequencyReports([FromBody] ManagementPeriodicReport report)
        {
            await _bl.Reports.UpdateFrequencyReport(report);
            return Ok();
        }

        [HttpDelete]
        [Route("FrequencyReports/{frequencyReportId}")]
        public async Task<IActionResult> DeleteFrequencyReports(Guid frequencyReportId)
        {
            await _bl.Reports.DeleteFrequencyReport(frequencyReportId);
            return Ok();
        }

        #region groups from company
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllGroupsDTO))]
        [Route("CompanyGroups/{companyId}")]
        public async Task<IActionResult> ReadGroupsByCompany(Guid companyId)
        {
            (var result, int totalCount) = await _bl.Reports.GetGroupsByCompany(companyId);
            var data = new List<GroupDTO>();
            foreach (var group in result)
            {
                data.Add(new GroupDTO(group));
            }
            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AllGroupsDTO { groups = data });
        }
        #endregion

        private IActionResult ExportCSV(IEnumerable<object> result, string title)
        {
            byte[] file = _bl.Reports.ReportToCsv(result);
            string fileName = $"{title}_Reports_{DateTime.Now.ToShortTimeString()}";
            Response.Headers.Add("x-file-name", fileName);
            return File(new MemoryStream(file), MediaTypeNames.Application.Octet, $"{fileName}.csv");
        }
    }
}
