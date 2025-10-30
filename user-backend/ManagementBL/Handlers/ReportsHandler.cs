using Common.Consts;
using Common.Enums.Companies;
using Common.Enums.Documents;
using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ManagementApp.Reports;
using Common.Models.Programs;
using Microsoft.Extensions.DependencyInjection;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces.Reports;
using Common.Enums.Reports;
using Common.Models.ManagementApp;
using Newtonsoft.Json;


namespace ManagementBL.Handlers
{
    public class ReportsHandler : IReport
    {
        private readonly GeneralSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDater _dater;
        private readonly Common.Interfaces.ManagementApp.IUsers _users;
        private readonly IManagementHistoryReports _managementHistoryReports;
        private readonly IHistoryDocumentCollection _historyDocumentCollection;

        public ReportsHandler(IOptions<GeneralSettings> options, IServiceScopeFactory scopeFactory, IDater dater,
            Common.Interfaces.ManagementApp.IUsers users, IManagementHistoryReports reportsHttpClientWrapper, IHistoryDocumentCollection historyDocumentCollection)
        {
            _settings = options.Value;
            _scopeFactory = scopeFactory;
            _dater = dater;
            _users = users;
            _managementHistoryReports = reportsHttpClientWrapper;
            _historyDocumentCollection = historyDocumentCollection;
        }

        #region Utilization

        public List<ProgramUtilizationHistory> Read(string key, int offset, int limit, DateTime? from, DateTime? to, out int totalCount)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }
            totalCount = 0;
            using var scope = _scopeFactory.CreateScope();
            IProgramUtilizationHistoryConnector programUtilizationHistoryConnector = scope.ServiceProvider.GetService<IProgramUtilizationHistoryConnector>();
            List<ProgramUtilizationHistory> result = new List<ProgramUtilizationHistory>();
            var programUtilizationHistories = programUtilizationHistoryConnector.Read(key, offset, limit, out totalCount);
            programUtilizationHistories = programUtilizationHistories.Where(x => x.UpdateDate >= from.Value && x.UpdateDate <= to.Value);
            foreach (var programUtilizationHistory in programUtilizationHistories)
            {
                var elInResult = result.FirstOrDefault(x => x.CompanyId == programUtilizationHistory.CompanyId);
                if (elInResult == null)
                {
                    result.Add(programUtilizationHistory);
                }
                else
                {
                    elInResult.DocumentsUsage += programUtilizationHistory.DocumentsUsage;
                    elInResult.SmsUsage += programUtilizationHistory.SmsUsage;
                    elInResult.TemplatesUsage = Math.Max(elInResult.TemplatesUsage, programUtilizationHistory.TemplatesUsage);
                    elInResult.UsersUsage = Math.Max(elInResult.UsersUsage, programUtilizationHistory.UsersUsage);
                }
            }
            totalCount = result.Count;

            return result.Skip(offset).Take(limit).ToList();
        }

        public async Task<(List<CompanyUtilizationReport>, int totalCount)> GetUtilizationReports(bool? isExpired, int monthsForAvgUse, Guid? programId, DateTime? from, DateTime? to, int offset, int limit)
        {
            int totalCount;
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0 && limit != Consts.UNLIMITED)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }
            using var scope = _scopeFactory.CreateScope();
            IProgramUtilizationHistoryConnector programUtilizationHistoryConnector = scope.ServiceProvider.GetService<IProgramUtilizationHistoryConnector>();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            List<CompanyUtilizationReport> result = new List<CompanyUtilizationReport>();
            List<ProgramUtilizationHistory> programUtilizationHistoriesSummed = new List<ProgramUtilizationHistory>();
            var programUtilizationHistories = programUtilizationHistoryConnector.Read(isExpired, programId, from.Value, to.Value);

            foreach (var companyUtilizationHistory in programUtilizationHistories.GroupBy(p => p.CompanyId))
            {
                Company company = await companyConnector.Read(new Company { Id = companyUtilizationHistory.Key });
                if (company != null)
                {
                    var tempgroup = companyUtilizationHistory.OrderByDescending(p => p.UpdateDate).ToList();
                    CompanyUtilizationReport companyUtilizationReport = new CompanyUtilizationReport(tempgroup, company.ProgramUtilization.StartDate, monthsForAvgUse);
                    if (tempgroup[0].Expired > _dater.UtcNow())
                    {
                        companyUtilizationReport.SmsUsage += company.ProgramUtilization.SMS;
                        companyUtilizationReport.DocumentsUsage += company.ProgramUtilization.DocumentsUsage;
                    }

                    result.Add(companyUtilizationReport);
                }

            }
            totalCount = result.Count;
            return ((limit == Consts.UNLIMITED ? result.Skip(offset).ToList() : result.Skip(offset).Take(limit).ToList()), totalCount);
        }

        public async Task<(List<CompanyUtilizationReport>, int totalCount)> GetUtilizationReportsByProgram(Guid programID, int monthsForAvgUse, int minDocs, int minSMS, int offset, int limit)
        {

            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }

            using var scope = _scopeFactory.CreateScope();
            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            Program program = await programConnector.Read(new Program { Id = programID });
            if (program == null)
            {
                throw new InvalidOperationException(ResultCode.ProgramNotExist.GetNumericString());
            }

            var companies = companyConnector.Read(programID, offset, limit, out int totalCount);
            (var companiesUtilizations, _) = (GetCompaniesUtilizations(companies, monthsForAvgUse, offset, limit, out totalCount), totalCount);
            companiesUtilizations = companiesUtilizations.Where(_ => _.DocumentsUsage >= minDocs && _.SmsUsage >= minSMS).ToList();
            totalCount = companiesUtilizations.Count;
            return (companiesUtilizations, totalCount);
        }

        public List<CompanyUtilizationReport> GetUtilizationReportsByPercentage(int docsUsagePercentage, int monthsOfUse, int offset, int limit, out int totalCount)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            var companies = companyConnector.Read(docsUsagePercentage, offset, limit, out totalCount);
            return GetCompaniesUtilizations(companies, monthsOfUse, offset, limit, out totalCount);
        }


        public List<CompanyUtilizationReport> GetAllCompaniesUtilizations(int monthsForAvgUse, int offset, int limit, out int totalCount)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            var companies = companyConnector.Read(null, offset, limit, CompanyStatus.Created, out totalCount);
            return GetCompaniesUtilizations(companies, monthsForAvgUse, offset, limit, out totalCount);

        }

        private List<CompanyUtilizationReport> GetCompaniesUtilizations(IEnumerable<Company> companies, int monthsForAvgUse, int offset, int limit, out int totalCount)
        {
            var reports = new List<CompanyUtilizationReport>();
            using var scope = _scopeFactory.CreateScope();
            IProgramUtilizationHistoryConnector programUtilizationHistoryConnector = scope.ServiceProvider.GetService<IProgramUtilizationHistoryConnector>();



            foreach (Company company in companies ?? new List<Company>())
            {
                var companyUtilizationHistories = programUtilizationHistoryConnector.Read(null, null, null, null, company.Id).ToList();
                if (companyUtilizationHistories.Count > 0)
                {
                    if (companyUtilizationHistories[0].Expired > _dater.UtcNow())
                    {
                        CompanyUtilizationReport companyUtilizationReport = new CompanyUtilizationReport(companyUtilizationHistories, company.ProgramUtilization, monthsForAvgUse);
                        reports.Add(companyUtilizationReport);
                    }
                    else
                    {
                        CompanyUtilizationReport companyUtilizationReport = new CompanyUtilizationReport(companyUtilizationHistories, company.ProgramUtilization.StartDate, monthsForAvgUse);
                        reports.Add(companyUtilizationReport);
                    }
                }
                else if (company.ProgramUtilization != null && company.ProgramUtilization.Expired > _dater.UtcNow())
                {
                    CompanyUtilizationReport companyUtilizationReport = new CompanyUtilizationReport(company, company.ProgramUtilization);
                    reports.Add(companyUtilizationReport);
                }
            }
            reports = reports.Skip(offset).Take(limit).ToList();
            totalCount = reports.Count;
            return reports;
        }
        public async Task<(List<GroupUtilizationReport>, int totalCount)> GetUtilizationReportPerGroup(Guid companyId, int docsUsagePercentage, int monthsOfUse, int offset, int limit)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();

            Company company = await companyConnector.Read(new Company() { Id = companyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            var companyGroups = groupConnector.Read(company);
            var result = new List<GroupUtilizationReport>();
            foreach (Group group in companyGroups ?? Enumerable.Empty<Group>())
            {
                GroupUtilizationReport groupUtilizationReport = new GroupUtilizationReport()
                {
                    GroupId = group.Id,
                    GroupName = group.Name
                };
                var docs = documentCollectionConnector.Read(group).Where(doc => doc.CreationTime >= DateTime.Today.AddMonths(-monthsOfUse));
                var oldDocs = await _historyDocumentCollection.ReadOldDocuments(DateTime.Today.AddMonths(-monthsOfUse), DateTime.MaxValue, null);
                var totalDocs = ConcatDocumentCollections(docs, oldDocs);
                foreach (DocumentCollection doc in totalDocs)
                {
                    groupUtilizationReport.PeriodicDocumentUsage++;
                    groupUtilizationReport.PeriodicSMSUsage += doc.Signers.Count(s => s.SendingMethod == SendingMethod.SMS);
                }
                result.Add(groupUtilizationReport);
            }
            result = result.Where(g => g.PeriodicDocumentUsage >= company.ProgramUtilization.DocumentsLimit * docsUsagePercentage / 100).ToList();
            int totalCount = result.Count;
            return (result.Skip(offset).Take(limit).ToList(), totalCount);
        }

        #endregion

        #region Programs
        public List<Program> GetProgramsReport(int minDocs, int minSMS, bool? isUsed, int offset, int limit, out int totalCount)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            var result = programConnector.Read(minDocs, minSMS, isUsed, offset, limit, out totalCount).ToList();

            return result;
        }

        #endregion

        #region DocCount Per Group
        public async Task<(List<GroupDocumentReport>, int totalCount)> GetGroupDocumentReports(Guid companyId, int offset, int limit)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();

            Company company = await companyConnector.Read(new Company { Id = companyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            var groups = groupConnector.Read(company);

            var result = new List<GroupDocumentReport>();
            foreach (Group group in groups ?? Enumerable.Empty<Group>())
            {
                GroupDocumentReport groupDocumentReport = new GroupDocumentReport()
                {
                    GroupName = group.Name
                };
                //// TODO: Check if valid
                var docs = documentCollectionConnector.Read(group);
                var oldDocs = await _historyDocumentCollection.ReadOldDocuments(DateTime.MinValue, DateTime.MaxValue, null, new List<Guid>() { group.Id });
                var totalDocs = ConcatDocumentCollections(docs, oldDocs);
                totalDocs.ForEach(doc => AddDocStatusToReport(groupDocumentReport, doc));
                result.Add(groupDocumentReport);
            }

            int totalCount = result.Count;
            return (result.Skip(offset).Take(limit).ToList(), totalCount);

        }

        private void AddDocStatusToReport(GroupDocumentReport groupDocumentReport, DocumentCollection documentCollection)
        {
            switch (documentCollection.DocumentStatus)
            {
                case DocumentStatus.Created:
                    groupDocumentReport.CreatedDocs++;
                    return;
                case DocumentStatus.Sent:
                    groupDocumentReport.SentDocs++;
                    return;
                case DocumentStatus.Viewed:
                    groupDocumentReport.ViewedDocs++;
                    return;
                case DocumentStatus.Signed:
                    groupDocumentReport.SignedDocs++;
                    return;
                case DocumentStatus.Declined:
                    groupDocumentReport.DeclinedDocs++;
                    return;
                case DocumentStatus.Deleted:
                    groupDocumentReport.DeletedDocs++;
                    return;
                case DocumentStatus.Canceled:
                    groupDocumentReport.CanceledDocs++;
                    return;
                case DocumentStatus.ExtraServerSigned:
                    groupDocumentReport.SignedDocs++;
                    return;
            }
        }
        #endregion

        #region DocCount Per User/Contact
        public async Task<(List<UserDocumentsReport>, int totalCount)> GetUserDocumentReports(Guid companyId, List<Guid> groupIds, bool isUser, DateTime? from, DateTime? to, int offset, int limit)
        {
            int totalCount;
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }

            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            Company company = await companyConnector.Read(new Company() { Id = companyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            List<Group> requestedGroups = new List<Group>();
            if (groupIds != null)
            {
                groupIds.ForEach(id => { requestedGroups.Add(new Group() { Id = id, CompanyId = company.Id }); });
            }
            else
            {
                (var requestedGroupsEnum, var _) = await GetGroupsByCompany(companyId);
                requestedGroups = requestedGroupsEnum.ToList();
                groupIds = requestedGroups.Select(_ => _.Id).ToList();
            }
            List<Group> groups = groupConnector.ReadMany(requestedGroups).ToList();
            if (groups.Count == 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
            }

            var oldDocs = await _historyDocumentCollection.ReadOldDocuments(from.Value, to.Value, null, groupIds);
            var docs = documentCollectionConnector.ReadByGroups(groups);
            var totalDocs = ConcatDocumentCollections(oldDocs, docs);
            totalDocs = totalDocs.Where(x => x.CreationTime >= from.Value && x.CreationTime <= to.Value);

            List<UserDocumentsReport> result = new List<UserDocumentsReport>();
            if (isUser)
            {

                Dictionary<Guid, string> usersName = new Dictionary<Guid, string>();
                if (totalDocs.Count() > 0)
                {
                    foreach (var groupDoc in totalDocs.GroupBy(d => d.User.Id))
                    {
                        string userName = "";
                        if (usersName.ContainsKey(groupDoc.Key))
                        {
                            userName = usersName[groupDoc.Key];
                        }
                        else
                        {
                            var user = await userConnector.Read(new User() { Id = groupDoc.Key });
                            if (user != null)
                            {
                                usersName.Add(user.Id, user.Name);
                            }
                            else
                            {
                                var userInDocs = totalDocs.FirstOrDefault(x => x.UserId == groupDoc.Key);
                                if (userInDocs != null)
                                {
                                    user = await userConnector.Read(new User() { Email = userInDocs.User?.Email ?? "" });
                                    if (user != null)
                                    {
                                        userName = user.Name;
                                    }
                                }



                                usersName.Add(groupDoc.Key, userName);
                            }
                            userName = usersName[groupDoc.Key];
                        }


                        result.Add(new UserDocumentsReport()
                        {
                            ContactName = userName,
                            DocumentAmount = groupDoc.Count()

                        });
                    }
                }
                totalCount = result.Count;

                return (result.Skip(offset).Take(limit).ToList(), totalCount);
            }
            else
            {
                var resultDict = new Dictionary<Guid, UserDocumentsReport>();
                foreach (var doc in totalDocs)
                {
                    foreach (var signer in doc.Signers)
                    {
                        if (!resultDict.ContainsKey(signer.Contact.Id))
                        {
                            resultDict.Add(signer.Contact.Id, new UserDocumentsReport()
                            {
                                ContactName = signer.Contact.Name,
                                DocumentAmount = 1
                            });
                        }
                        else resultDict[signer.Contact.Id].DocumentAmount++;
                    }
                }
                foreach (var userDocReport in resultDict.Values)
                {
                    result.Add(userDocReport);
                }
                totalCount = result.Count;
                return (result.Skip(offset).Take(limit).ToList(), totalCount);
            }
        }

        #endregion

        public async Task<(List<CompanyUserReport>, int totalCount)> GetUsersByCompany(Guid companyId, int offset, int limit)
        {
            int totalCount;
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }

            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }


            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

            Company company = await companyConnector.Read(new Company() { Id = companyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            List<CompanyUserReport> result = new List<CompanyUserReport>();
            var groups = groupConnector.Read(company).ToList();
            foreach (Group group in groups)
            {
                var groupDocs = documentCollectionConnector.Read(group).ToList();
                var oldDocs = _historyDocumentCollection.ReadOldDocuments(DateTime.MinValue, DateTime.MaxValue, null, new List<Guid>() { group.Id });
                HashSet<Guid> users = new HashSet<Guid>();

                foreach (var groupDoc in groupDocs.GroupBy(d => d.User.Id))
                {
                    User user = await userConnector.Read(new User() { Id = groupDoc.Key });
                    CompanyUserReport userCompanyUserReport = new CompanyUserReport()
                    {
                        UserName = user?.Name ?? "",
                        Email = user?.Email ?? "",
                        DocumentAmounts = groupDoc.Count(),
                        GroupName = group?.Name ?? ""
                    };
                    if (userCompanyUserReport.UserName == "")
                        continue;

                    result.Add(userCompanyUserReport);
                    users.Add(user.Id);
                }

                foreach (User user in userConnector.Read(group).ToList())
                {
                    if (!users.Contains(user.Id))
                    {
                        users.Add(user.Id);
                        result.Add(new CompanyUserReport()
                        {
                            UserName = user?.Name ?? "",
                            Email = user?.Email ?? "",
                            DocumentAmounts = 0,
                            GroupName = group?.Name ?? ""
                        });
                    }
                }
            }
            totalCount = result.Count;
            return (result.Skip(offset).Take(limit).ToList(), totalCount);
        }

        public List<FreeTrialUserReport> GetFreeTrialUsers(int offset, int limit, out int totalCount)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }

            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }


            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

            var users = userConnector.ReadFreeTrialUsers();
            List<FreeTrialUserReport> result = new List<FreeTrialUserReport>();
            foreach (var user in users)
            {
                if (user.ProgramUtilization != null)
                {
                    FreeTrialUserReport userReport = new FreeTrialUserReport()
                    {
                        Name = user.Name,
                        Email = user.Email,
                        UserName = user.Username,
                        CreationDate = user.CreationTime,
                        DocumentsUsage = Convert.ToInt32(user.ProgramUtilization.DocumentsUsage),
                        SMSUsage = Convert.ToInt32(user.ProgramUtilization.SMS),
                        TemplatesUsage = Convert.ToInt32(user.ProgramUtilization.Templates),
                        ExpirationDate = user.ProgramUtilization.Expired,
                    };
                    result.Add(userReport);
                }
            }
            totalCount = result.Count;

            return result.Skip(offset).Take(limit).ToList();

        }

        public async Task<(IEnumerable<UsageByUserReport>, int totalCount)> GetUsageByUsers(string userEmail, Guid companyId, List<Guid> groupIds, DateTime? from, DateTime? to, int offset, int limit)
        {
            if (userEmail == null && companyId == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }

            using var scope = _scopeFactory.CreateScope();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
            var company = new Company() { Id = companyId };
            var oldReports = await _managementHistoryReports.ReadUsageByUserDetails(userEmail, company, from.Value, to.Value, groupIds);
            var usageByUsersReports = documentCollectionConnector.ReadUsageByUserDetails(userEmail, company, groupIds, from.Value, to.Value);
            var totalReports = (await MergeUsageByUsersReports(oldReports, usageByUsersReports)).ToList();
            int totalCount = totalReports.Count;
            return (totalReports.Skip(offset).Take(limit), totalCount);
        }

        private async Task<IEnumerable<UsageByUserReport>> MergeUsageByUsersReports(IEnumerable<UsageByUserReport> reports, IEnumerable<UsageByUserReport> additionalReports)
        {
            using var scope = _scopeFactory.CreateScope();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            var allReports = reports.Concat(additionalReports);
            var mergedReports = allReports
                .GroupBy(r => new { r.CompanyName, r.GroupId, r.Email })
                .Select(g => new UsageByUserReport
                {
                    CompanyName = g.Key.CompanyName,
                    GroupId = g.Key.GroupId,
                    Email = g.Key.Email,
                    SentDocumentsCount = g.Sum(r => r.SentDocumentsCount),
                    SignedDocumentsCount = g.Sum(r => r.SignedDocumentsCount),
                    DeclinedDocumentsCount = g.Sum(r => r.DeclinedDocumentsCount),
                    CanceledDocumentsCount = g.Sum(r => r.CanceledDocumentsCount),
                    DeletedDocumentsCount = g.Sum(r => r.DeletedDocumentsCount)
                }).ToList();
            foreach (var report in mergedReports)
            {
                var dbGroup = await groupConnector.Read(new Group() { Id = report.GroupId });
                report.GroupName = dbGroup.Name;
            }

            return mergedReports;
        }

        public async Task<(IEnumerable<UsageByCompanyReport>, int totalCount)> GetUsageByCompanies(Guid companyId, List<Guid> groupIds, DateTime? from, DateTime? to, int offset, int limit)
        {
            if (companyId == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }

            var company = new Company() { Id = companyId };

            using var scope = _scopeFactory.CreateScope();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
            var oldReports = await _managementHistoryReports.ReadUsageByCompanyAndGroups(company, groupIds, from.Value, to.Value);
            var usageByCompaniesReports = documentCollectionConnector.ReadUsageByCompanyAndGroups(company, groupIds, from.Value, to.Value);
            var totalReports = (await MergeUsageByCompaniesReports(oldReports, usageByCompaniesReports)).ToList();
            int totalCount = totalReports.Count;
            return (totalReports.Skip(offset).Take(limit), totalCount);
        }

        private async Task<IEnumerable<UsageByCompanyReport>> MergeUsageByCompaniesReports(IEnumerable<UsageByCompanyReport> reports, IEnumerable<UsageByCompanyReport> additionalReports)
        {
            using var scope = _scopeFactory.CreateScope();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            var allReports = reports.Concat(additionalReports);
            var mergedReports = allReports
                .GroupBy(r => new { r.CompanyName, r.GroupId })
                .Select(g => new UsageByCompanyReport
                {
                    CompanyName = g.Key.CompanyName,
                    GroupId = g.Key.GroupId,
                    SentDocumentsCount = g.Sum(r => r.SentDocumentsCount),
                    SignedDocumentsCount = g.Sum(r => r.SignedDocumentsCount),
                    DeclinedDocumentsCount = g.Sum(r => r.DeclinedDocumentsCount),
                    CanceledDocumentsCount = g.Sum(r => r.CanceledDocumentsCount)
                }).ToList();
            foreach (var report in mergedReports)
            {
                var dbGroup = await groupConnector.Read(new Group() { Id = report.GroupId });
                if (dbGroup != null)
                {
                    report.GroupName = dbGroup.Name;
                }
            }
            return mergedReports;
        }

        public async Task<(List<TemplatesByUsageReport>, int totalCount)> GetTemplatesByUsage(Guid companyId, List<Guid> groupIds, DateTime? from, DateTime? to, int offset, int limit)
        {

            if (companyId == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }

            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            ITemplateConnector templateConnector = scope.ServiceProvider.GetService<ITemplateConnector>();

            var company = await companyConnector.Read(new Company() { Id = companyId });

            var templatesByUsageReports = templateConnector.ReadTemplatesByUsage(companyId, groupIds, from.Value, to.Value, offset, limit, out int totalCount).ToList();
            foreach (var report in templatesByUsageReports)
            {
                report.CompanyName = company.Name;
                var group = await groupConnector.Read(new Group() { Id = report.GroupId });
                report.GroupName = group.Name;
            }

            return (templatesByUsageReports, totalCount);
        }

        public async Task<UsageBySignatureTypeReport> GetUsageBySignatureType(Guid companyId, List<SignatureFieldType> signatureTypes, DateTime? from, DateTime? to)
        {
            if (companyId == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }

            using var scope = _scopeFactory.CreateScope();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();

            var company = new Company() { Id = companyId };
            var oldReport = await _managementHistoryReports.ReadUsageByCompanyAndSignatureTypes(company, signatureTypes, from.Value, to.Value);
            var usageBySignatureTypesReport = await documentCollectionConnector.ReadUsageByCompanyAndSignatureTypes(company, signatureTypes, from.Value, to.Value);
            var report = MergeUsageBySignatureTypeReports(oldReport, usageBySignatureTypesReport);
            return usageBySignatureTypesReport;
        }

        private UsageBySignatureTypeReport MergeUsageBySignatureTypeReports(UsageBySignatureTypeReport report, UsageBySignatureTypeReport historyReport)
        {
            if (historyReport == null)
                return report;

            if (report == null)
                return historyReport;

            return new UsageBySignatureTypeReport()
            {
                CompanyName = report.CompanyName,
                ServerFieldsCount = report.ServerFieldsCount + historyReport.ServerFieldsCount,
                GraphicFieldsCount = report.GraphicFieldsCount + historyReport.GraphicFieldsCount,
                SmartCardFieldsCount = report.SmartCardFieldsCount + historyReport.SmartCardFieldsCount
            };
        }

        #region groupsByCompany (not a report)
        public async Task<(List<Group>, int totalCount)> GetGroupsByCompany(Guid companyId)
        {
            int totalCount;


            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();


            Company company = await companyConnector.Read(new Company() { Id = companyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            var result = groupConnector.Read(company);
            totalCount = result.Count();
            return (result.ToList(), totalCount);
        }

        #endregion

        #region csv

        public byte[] ReportToCsv(IEnumerable<object> reportList)
        {
            return CsvHandler.ExportDocumentsCollection(reportList, Common.Enums.Users.Language.en);
        }

        #endregion

        private IEnumerable<DocumentCollection> ConcatDocumentCollections(IEnumerable<DocumentCollection> firstCollection, IEnumerable<DocumentCollection> secondCollection)
        {
            if (firstCollection == null && secondCollection == null)
            {
                return Enumerable.Empty<DocumentCollection>();
            }
            if (firstCollection == null)
            {
                return secondCollection ?? Enumerable.Empty<DocumentCollection>();
            }
            return secondCollection != null ? firstCollection.Concat(secondCollection) : firstCollection;
        }

        public async Task CreateFrequencyReport(ReportParameters reportParameters, ManagementReportFrequency reportFrequency, ManagementReportType reportType, List<string> emails)
        {
            var user = await _users.GetCurrentUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            if (emails == null || !emails.Any())
            {
                throw new InvalidOperationException(ResultCode.EmailsRequiredToManagementPeriodicReports.GetNumericString());
            }
            bool isDuplicatedEmails = emails.Count != emails.Distinct().Count();
            if (isDuplicatedEmails)
            {
                throw new InvalidOperationException(ResultCode.DuplicatedPeriodicReportEmails.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            IManagementPeriodicReportConnector managementPeriodicReportConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportConnector>();
            IManagementPeriodicReportEmailConnector managementPeriodicReportEmailConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportEmailConnector>();
            var reportParamsStr = JsonConvert.SerializeObject(reportParameters);
            var emailsToSend = new List<ManagementPeriodicReportEmail>();

            if (reportParameters != null)
            {
                reportParameters.Limit = int.MaxValue;
            }

            if (reportParameters != null && reportParameters.GroupIds != null && !reportParameters.GroupIds.Any())
            {
                reportParameters.GroupIds = null;
            }

            // Create the periodic report
            var managementPeriodicReport = new ManagementPeriodicReport()
            {
                UserId = user.Id,
                ReportType = reportType,
                LastTimeSent = _dater.MinValue(),
                ReportFrequency = reportFrequency,
                ReportParameters = reportParamsStr,
                User = user
            };
            var reportId = await managementPeriodicReportConnector.Create(managementPeriodicReport);

            try
            {
                // Create the emails
                foreach (var email in emails)
                {
                    var emailReport = new ManagementPeriodicReportEmail()
                    {
                        Email = email,
                        PeriodicReportId = reportId,
                        Report = managementPeriodicReport
                    };
                    managementPeriodicReportEmailConnector.Create(emailReport);
                    managementPeriodicReport.Emails.Add(emailReport);
                }
            }
            catch (Exception)
            {
                managementPeriodicReportConnector.Delete(reportId);
                managementPeriodicReportEmailConnector.DeleteAllByReportId(reportId);
                throw new InvalidOperationException(ResultCode.FailedCreateManagementPeriodicReportMail.GetNumericString());
            }

        }

        public async Task<IEnumerable<ManagementPeriodicReport>> ReadFrequencyReports()
        {
            var user = await _users.GetCurrentUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            using var scope = _scopeFactory.CreateScope();
            IManagementPeriodicReportConnector managementPeriodicReportConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportConnector>();
            var reports = managementPeriodicReportConnector.ReadAll();
            return reports;
        }

        public async Task UpdateFrequencyReport(ManagementPeriodicReport report)
        {
            var user = await _users.GetCurrentUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            if (report.Emails == null || report.Emails.Count == 0)
            {
                throw new InvalidOperationException(ResultCode.EmailsRequiredToManagementPeriodicReports.GetNumericString());
            }

            using var scope = _scopeFactory.CreateScope();
            IManagementPeriodicReportConnector managementPeriodicReportConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportConnector>();
            IManagementPeriodicReportEmailConnector managementPeriodicReportEmailConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportEmailConnector>();
            managementPeriodicReportEmailConnector.DeleteAllByReportId(report.Id);
            await managementPeriodicReportConnector.Update(report);
            report.Emails.ForEach(email =>
            {
                managementPeriodicReportEmailConnector.Create(email);
            });

        }

        public async Task DeleteFrequencyReport(Guid id)
        {
            var user = await _users.GetCurrentUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            using var scope = _scopeFactory.CreateScope();
            IManagementPeriodicReportConnector managementPeriodicReportConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportConnector>();
            IManagementPeriodicReportEmailConnector managementPeriodicReportEmailConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportEmailConnector>();
            managementPeriodicReportEmailConnector.DeleteAllByReportId(id);
            managementPeriodicReportConnector.Delete(id);
        }
    }
}
