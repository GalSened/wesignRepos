
using Common.Enums.Program;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.ManagementApp;
using Common.Models.Programs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class PaymentHandler : IPayment
    {
        
        private readonly IPrograms _programs;
        private readonly Interfaces.ManagementApp.IUsers _users;
        private readonly ILogger _logger;
        private readonly ICompanies _companies;
        private readonly IDater _dater;
        private IProgramConnector _programConnector;
        private ICompanyConnector _companyConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;

        public PaymentHandler (IProgramConnector programConnector, ICompanyConnector companyConnector, IProgramUtilizationConnector programUtilizationConnector
            ,IPrograms programs, Interfaces.ManagementApp.IUsers users, ICompanies companies, IDater dater, ILogger logger)
        {

            _programConnector = programConnector;
            _companyConnector = companyConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _programs = programs;
            _users = users;
            _companies = companies;
            _dater = dater;
            _logger = logger;
        }

        public async Task UnsubscribeCompany(Company company)
        {
            if (company == null || company.Id == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            Company companyFromDB = await _companyConnector.Read(company);
            if (companyFromDB == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            company.TransactionId = "";
            await _companies.UpdateTransactionId(company);
        }


        public async Task UpdateCompanyTransactionAndExpirationTime(Company company)
        {
            if (company == null || company.Id == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            Company companyFromDB = await _companyConnector.Read(company);
            if (companyFromDB == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            companyFromDB.TransactionId = company.TransactionId;
            companyFromDB.ProgramUtilization.Expired = company.ProgramUtilization.Expired;
            await _companies.UpdateCompanyTransactionAndExpirationTime(companyFromDB);

        }
        public async Task UpdateRenwablePayment(UpdatePaymentRenewable userPayment)
        {
            User user = await _users.Read(new User() { Email = userPayment.Email });
            if (user == null)
            {
                _logger.Error("UserPay - User not exist [{Email}]", userPayment.Email);
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            CompanyExpandedDetails company =  await _companies.Read(new Company { Id = user.CompanyId }, user);
            if (company == null)
            {                
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            if(company.TransactionId != userPayment.OldTransactionId)
            {
                throw new InvalidOperationException(ResultCode.InvalidTransactionId.GetNumericString());
            }
                        
            await _companies.UpdateTransactionId(new Company { Id = company.Id, TransactionId = userPayment.TransactionId });

        }

        public async Task UserPay(UserPayment userPayment)
        {

            User user = await _users.Read(new User() { Email = userPayment.UserEmail });
            if(user == null)
            {                
                _logger.Error("UserPay - User not exist [{Email}]", userPayment.UserEmail);
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            Program program =await _programs.Read(new Program() { Id = userPayment.ProgramID });
            if(program == null)
            {
                throw new InvalidOperationException(ResultCode.ProgramNotExist.GetNumericString());
            }

            if(_programConnector.IsFreeTrialUser(user))
            {
                await HandleFreeTrailUser(user, program, userPayment);

                
            }
            else
            {
                await HandleUser(user, program, userPayment, !string.IsNullOrEmpty(userPayment.TransactionId));
            }
            
        }

        private async Task HandleUser(User user, Program program, UserPayment userPayment, bool isRenewalPlan)
        {
            Company company =await _companyConnector.Read(new Company() { Id = user.CompanyId });
            company.TransactionId = userPayment.TransactionId;
            company.ProgramId = program.Id;
            await _companyConnector.Update(company);

            ProgramUtilization programUtilization = await _programUtilizationConnector.Read(new ProgramUtilization() { Id = company.ProgramUtilization.Id });
            ProgramResetType prevProgramResetType = programUtilization.ProgramResetType;
            programUtilization.ProgramResetType = userPayment.ProgramResetType;
            if (isRenewalPlan)
            {
                programUtilization.DocumentsLimit = program.DocumentsPerMonth;
            }
            else
            {
                programUtilization.DocumentsLimit += program.DocumentsPerMonth;
            }
            if (userPayment.ProgramResetType == ProgramResetType.DocumentsLimitOnly)
            {
                if (prevProgramResetType != ProgramResetType.DocumentsLimitOnly)
                {
                    programUtilization.Expired = DateTime.MaxValue.AddYears(-5);
                }
            }
            else
            {

                DateTime expiration = _dater.UtcNow().AddMonths(userPayment.MonthToAdd > 0 ? userPayment.MonthToAdd : 1);

                if (programUtilization.Expired > _dater.UtcNow() && (prevProgramResetType == ProgramResetType.TimeAndDocumentsLimit || prevProgramResetType == ProgramResetType.Monthly || prevProgramResetType == ProgramResetType.Yearly))
                {
                    if (!isRenewalPlan)
                    {
                        expiration = programUtilization.Expired.AddMonths(userPayment.MonthToAdd > 0 ? userPayment.MonthToAdd : 1);
                    }

                }
                programUtilization.Expired = expiration;
            }

            programUtilization.SMS = 0;
            programUtilization.DocumentsUsage = 0;
            programUtilization.VisualIdentifications = 0;
            programUtilization.VideoConference = 0;
            await _programUtilizationConnector.Update(programUtilization);
            
        }

        private async Task HandleFreeTrailUser(User user, Program program, UserPayment userPayment)
        {

            Company company = GetCompany(user, program, userPayment);
            Group group = new() { Name = company.Name };
           
            await _companies.Create(company, group, user);
            await _companies.ResendResetPasswordMail(user);
        }


        private Company GetCompany(User user, Program program, UserPayment userPayment)
        {
            CompanyConfiguration companyConfig = new CompanyConfiguration()
            {
                Language = user.UserConfiguration.Language,
                EmailTemplates = new EmailHtmlBodyTemplates(),
                SignatureColor = user.UserConfiguration.SignatureColor,
                ShouldSendSignedDocument = user.UserConfiguration.ShouldSendSignedDocument,
                DefaultSigningType = Enums.PDF.SignatureFieldType.Graphic
             
            };

            DateTime expiration = DateTime.MaxValue.AddYears(-5);
            if(userPayment.ProgramResetType == ProgramResetType.TimeAndDocumentsLimit || userPayment.ProgramResetType == ProgramResetType.Monthly || userPayment.ProgramResetType == ProgramResetType.Yearly)
            {
                expiration = _dater.UtcNow().AddMonths(userPayment.MonthToAdd > 0 ? userPayment.MonthToAdd : 1);
            }

            ProgramUtilization programUtilization = new ProgramUtilization()
            {
                DocumentsLimit = program.DocumentsPerMonth,
                Expired = expiration,
                ProgramResetType = userPayment.ProgramResetType,
                StartDate = _dater.UtcNow(),
                LastResetDate = _dater.UtcNow(),
            };


            Company company = new Company()
            {
                Name = $"{user.Email.Replace("@", "_")}-Company_{Guid.NewGuid().ToString("N").Substring(0, 5)}",
                Status = Enums.Companies.CompanyStatus.Created,
                ProgramId = program.Id,
                CompanyConfiguration = companyConfig,
                ProgramUtilization = programUtilization,
                TransactionId = userPayment.TransactionId

            };


            return company;
        }

       
    }
}
