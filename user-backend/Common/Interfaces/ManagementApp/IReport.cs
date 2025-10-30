using Common.Enums.PDF;
using Common.Enums.Reports;
using Common.Models;
using Common.Models.ManagementApp;
using Common.Models.ManagementApp.Reports;
using Common.Models.Programs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Common.Interfaces.ManagementApp
{
    public interface IReport
    {
        List<ProgramUtilizationHistory> Read(string key, int offset, int limit, DateTime? from, DateTime? to, out int totalCount);

        Task<(List<CompanyUtilizationReport>, int totalCount)> GetUtilizationReports(bool? isExpired, int monthsForAvgUse, Guid? programId, DateTime? from, DateTime? to, int offset, int limit);

        Task<(List<CompanyUtilizationReport>, int totalCount)> GetUtilizationReportsByProgram(Guid programID, int monthsForAvgUse, int minDocs, int minSMS, int offset, int limit);

        List<CompanyUtilizationReport> GetUtilizationReportsByPercentage(int docsUsagePercentage, int monthsOfUse, int offset, int limit, out int totalCount);

        Task<(List<GroupUtilizationReport>, int totalCount)> GetUtilizationReportPerGroup(Guid companyId, int docsUsagePercentage, int monthsOfUse, int offset, int limit);

        List<CompanyUtilizationReport> GetAllCompaniesUtilizations(int monthsForAvgUse, int offset, int limit, out int totalCount);

        List<Program> GetProgramsReport(int minDocs, int minSMS, bool? isUsed, int offset, int limit, out int totalCount);

        Task<(List<GroupDocumentReport>, int totalCount)> GetGroupDocumentReports(Guid companyId, int offset, int limit);

        Task<(List<UserDocumentsReport>, int totalCount)> GetUserDocumentReports(Guid companyId, List<Guid> groupIds, bool isUser, DateTime? from, DateTime? to, int offset, int limit);

        Task<(List<CompanyUserReport>, int totalCount)> GetUsersByCompany(Guid companyId, int offset, int limit);
        Task<(List<Group>, int totalCount)> GetGroupsByCompany(Guid companyId);

        List<FreeTrialUserReport> GetFreeTrialUsers(int offset, int limit, out int totalCount);
        Task<(IEnumerable<UsageByUserReport>, int totalCount)> GetUsageByUsers(string userEmail, Guid companyId, List<Guid> groupIds, DateTime? from, DateTime? to, int offset, int limit);
        Task<(IEnumerable<UsageByCompanyReport>, int totalCount)> GetUsageByCompanies(Guid companyId, List<Guid> groupIds, DateTime? from, DateTime? to, int offset, int limit);
        Task<(List<TemplatesByUsageReport>, int totalCount)> GetTemplatesByUsage(Guid companyId, List<Guid> groupIds, DateTime? from, DateTime? to, int offset, int limit);
        Task<UsageBySignatureTypeReport> GetUsageBySignatureType(Guid companyId, List<SignatureFieldType> signatureTypes, DateTime? from, DateTime? to);
        byte[] ReportToCsv(IEnumerable<object> reportList);
        Task CreateFrequencyReport(ReportParameters reportParameters, ManagementReportFrequency reportFrequency, ManagementReportType reportType, List<string> emails);
        Task UpdateFrequencyReport(ManagementPeriodicReport managementPeriodicReport);
        Task<IEnumerable<ManagementPeriodicReport>> ReadFrequencyReports();
        Task DeleteFrequencyReport(Guid id);
    }
}
