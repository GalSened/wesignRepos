using Common.Enums.PDF;
using Common.Models;
using Common.Models.ManagementApp.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Reports
{
    public interface IManagementHistoryReports
    {
        Task<IEnumerable<UsageByUserReport>> ReadUsageByUserDetails(string UserEmail, Company company, DateTime from, DateTime to, IEnumerable<Guid> groupIds = null);
        Task<IEnumerable<UsageByCompanyReport>> ReadUsageByCompanyAndGroups(Company company, IEnumerable<Guid> groupIds, DateTime from, DateTime to);
        Task<UsageBySignatureTypeReport> ReadUsageByCompanyAndSignatureTypes(Company company, IEnumerable<SignatureFieldType> signatureFieldTypes, DateTime from, DateTime to);
    }
}
