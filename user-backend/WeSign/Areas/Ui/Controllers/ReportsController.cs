using Common.Enums.Documents;
using Common.Enums.Reports;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.Reports;
using Common.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Threading.Tasks;
using WeSign.Extensions;
using WeSign.Models.Reports.Response;

namespace WeSign.areas.ui.Controllers
{
//#if DEBUG
//    [Route("userui/v3/reports")]
//#else
//    [Route("ui/v3/reports")]
//#endif
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ReportsController : ControllerBase
    {
        private readonly IReports _reports;

        public ReportsController(IReports reports)
        {
            _reports = reports;
        }

        /// <summary>
        /// Read user usage data reports
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="docStatuses"></param>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("UsageData")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUsageDataReportsDTO))]
        public async Task<IActionResult> ReadUsageData([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] List<DocumentStatus> docStatuses = null, [FromQuery] List<Guid> groupIds = null,
            [FromQuery] bool includeDistributionDocs = false, [FromQuery] int offset = 0, [FromQuery] int limit = 20, [FromQuery] bool isCSV = false)
        {
            var reports = await _reports.ReadUsageData(from, to, docStatuses, groupIds, includeDistributionDocs, offset, limit);
            if (!isCSV)
            {
                var response = new List<UsageDataReportDTO>();
                foreach (var report in reports)
                {
                    var usageDataReportDTO = new UsageDataReportDTO(report);
                    response.Add(usageDataReportDTO);
                }
                return Ok(new AllUsageDataReportsDTO() { usageDataReports = response });
            }
            else
            {
                return await ExportCSV(reports, "UsageData");
            }
        }

        /// <summary>
        /// Create frequency reports
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="reportTypeStr"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("FrequencyReports")]
        public async Task<IActionResult> CreateFrequencyReports([FromQuery] ReportFrequency frequency, [FromQuery] string reportTypeStr)
        {
            if (frequency != ReportFrequency.None)
            {
                ReportType? reportType = EnumExtensions.EnumFromString<ReportType>(reportTypeStr);
                if (reportType.HasValue)
                {
                    await _reports.CreateFrequencyReport(frequency, reportType.Value);
                    return Ok();
                }
                return BadRequest();
            }
            return NoContent();
        }

        /// <summary>
        /// Read frequency reports
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("FrequencyReports")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUserPeriodicReports))]
        public async Task<IActionResult> ReadFrequencyReports()
        {
            var periodicReports = (await _reports.ReadFrequencyReports()).ToList();
            if (periodicReports != null && periodicReports.Any())
                return Ok(new AllUserPeriodicReports() { userPeriodicReports = periodicReports });
            return NoContent();
        }

        /// <summary>
        /// Delete frequency reports
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("FrequencyReports")]
        public async Task<IActionResult> DeleteFrequencyReports()
        {
            await _reports.DeleteAllFrequencyReports();
            return Ok();
        }

        /// <summary>
        /// Download frequency reports
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("FrequencyReports/Download")]
        public async Task<IActionResult> DownloadFrequencyReports([FromQuery] Guid id)
        {
            var fileBytes = _reports.DownloadFrequencyReportsFile(id);
            if (fileBytes != null)
            {
                Response.Headers.Add("x-file-name", id.ToString());
                return File(new MemoryStream(fileBytes), MediaTypeNames.Application.Octet, $"{id}.csv");
            }
            return BadRequest();
        }

        private async Task<IActionResult> ExportCSV(IEnumerable<object> result, string title)
        {
            if (result != null && result.Any())
            {
                byte[] file = await _reports.ReportToCsv(result);
                string fileName = $"{title}_Reports_{DateTime.Now.ToShortTimeString()}";
                Response.Headers.Add("x-file-name", fileName);
                return File(new MemoryStream(file), MediaTypeNames.Application.Octet, $"{fileName}.csv");
            }
            else
            {
                return NoContent();
            }
        }

    }
}
