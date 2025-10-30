using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Programs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WeSignManagement.Models.ActiveDirectory;
using WeSignManagement.Models.Companies;
using WeSignManagement.Models.Companies.Responses;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/companies")]
#else
    [Route("v3/companies")]
#endif
 
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class CompaniesController : ControllerBase
    {
        private readonly IManagementBL _bl;
        private readonly IDater _dater;

        public CompaniesController(IManagementBL bl, IDater dater)
        {
            _bl = bl;
            _dater = dater;
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,PaymentAdmin")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(CompanyDTO input)
        {
            var user = new User
            {
                Name = input.User.UserName,
                Email = input.User.Email,
                Username = input.User.UserUsername
            };
            var group = new Group
            {
                Name = input.User.GroupName
            };
            var company = GetCompany(input);

            await _bl.Companies.Create(company, group, user);

            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = "SystemAdmin,PaymentAdmin")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(Guid id, CompanyDTO input)
        {
            var user = new User
            {
                Name = input.User?.UserName,
                Email = input.User?.Email,
                CompanyId = id,
                Username = input.User?.UserUsername
            };
            var group = new Group
            {
                Name = input.User?.GroupName,
                CompanyId = id
            };
            var company = GetCompany(input);
            company.Id = id;

            await _bl.Companies.Update(company, group, user);

            ICollection<ActiveDirectoryGroup> activeDirectoryGroups = GetADGroupsFromInput(input);
            await _bl.ActiveDirectory.AddUpdateCompanyADGroupsMapping(company, activeDirectoryGroups);

            return Ok();
        }
        
        /// <summary>
        /// Call for set company in Form 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/users/{userId}")]
        [Authorize(Roles = "SystemAdmin,PaymentAdmin")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CompanyExpandedResponseDTO))]
        public async Task<IActionResult> Read(Guid id, Guid userId)
        {
            var companyExpandedDetails = await _bl.Companies.Read(new Company { Id = id }, new User { Id = userId});

            return Ok(new CompanyExpandedResponseDTO(companyExpandedDetails));
        }

        [HttpGet]
        [Route("{id}/deletionconfiguration")]
        [Authorize(Roles ="SystemAdmin, PaymentAdmin")]
        [SwaggerResponse((int)HttpStatusCode.OK,Type = typeof(DeletionDTO))]
        public async Task<IActionResult> ReadDeletionConfiguration(Guid id)
        {
            var companyDeletionConfiguration =await _bl.Companies.ReadCompanyDeletionConfiguration(new Company() { Id = id });

            return Ok(new DeletionDTO(companyDeletionConfiguration));
        }

        /// <summary>
        /// Call for dashboard
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllCompaniesResponseDTO))]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Read(string key = null, int offset = 0, int limit = 20)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < -1)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            (var companiesDetails, int totalCount) =await _bl.Companies.Read(key, offset, limit );
            var response = new List<CompanyBaseResponseDTO>();
            foreach (var companyDetail in companiesDetails)
            {
                response.Add(new CompanyBaseResponseDTO(companyDetail));
            }

            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AllCompaniesResponseDTO() { Companies = response });
            
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "SystemAdmin")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var company = new Company
            {
                Id = id
            };
            await _bl.Companies.Delete(company);
            await _bl.ActiveDirectory.DeleteCompanyMappedGroup(company);
            return Ok();
        }

        /// <summary>
        /// Resend reset password word to company user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("password/{userId}")]
        [Authorize(Roles = "SystemAdmin")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResendResetPassword(Guid userId)
        {
            var user = new User
            {
                Id = userId
            };
            await _bl.Companies.ResendResetPasswordMail(user);

            return Ok();
        }

        #region Private Functions

        private Company GetCompany(CompanyDTO input)
        {
            int.TryParse(input?.SmtpConfiguration?.SmtpPort, out int port);
            return new Company
            {
                Name = input.CompanyName,
                TransactionId = input.TransactionId,
                CompanyConfiguration = new CompanyConfiguration
                {
                    Language = input.Language,
                    Base64Logo = input.LogoBase64String,
                    EmailTemplates = new EmailHtmlBodyTemplates
                    {
                        BeforeSigningBase64String = input.SmtpConfiguration?.BeforeSigningHtmlTemplateBase64String,
                        AfterSigningBase64String = input.SmtpConfiguration?.AfterSigningHtmlTemplateBase64String
                    },
                    SignatureColor = input.SignatureColor,
                    CompanyMessages = new List<CompanyMessage>()
                    {
                        new CompanyMessage
                        {
                            MessageType = MessageType.BeforeSigning,
                            Content = input.MessageBefore,
                            Language = Common.Enums.Users.Language.en,
                        },
                        new CompanyMessage
                        {
                            MessageType = MessageType.AfterSigning,
                            Content = input.MessageAfter,
                            Language = Common.Enums.Users.Language.en,
                        },
                        new CompanyMessage
                        {
                            MessageType = MessageType.AfterSigning,
                            Content = input.MessageAfterHebrew,
                            Language = Common.Enums.Users.Language.he,
                        },
                        new CompanyMessage
                        {
                            MessageType = MessageType.BeforeSigning,
                            Content = input.MessageBeforeHebrew,
                            Language = Common.Enums.Users.Language.he,
                        }
                    },
                    MessageProviders = new List<MessageProvider>
                    {
                        new MessageProvider
                        {
                            ProviderType = ProviderType.EmailSmtp,
                            From = input?.SmtpConfiguration?.SmtpFrom,
                            User = input?.SmtpConfiguration?.SmtpUser,
                            Password = input?.SmtpConfiguration?.SmtpPassword,
                            Server = input?.SmtpConfiguration?.SmtpServer,
                            Port =  port,
                            EnableSsl = input?.SmtpConfiguration?.SmtpEnableSsl ?? false
                        },
                        new MessageProvider
                        {
                            ProviderType = input?.SmsConfiguration?.Provider ?? ProviderType.SmsGoldman,
                            From = input?.SmsConfiguration?.From,
                            User = input?.SmsConfiguration?.User,
                            Password = input?.SmsConfiguration?.Password
                        }
                    },
                    ShouldForceOTPInLogin = input?.ShouldForceOTPInLogin ?? false,
                    ShouldEnableMeaningOfSignatureOption = input?.ShouldEnableMeaningOfSignatureOption ?? false,
                    ShouldSendSignedDocument = input.Notifications?.ShouldSendSignedDocument ?? false ,
                    ShouldEnableVideoConference = input?.ShouldEnableVideoConference ?? false,
                    ShouldNotifyWhileSignerSigned = input.Notifications?.ShouldNotifyWhileSignerSigned ?? false,
                    ShouldEnableSignReminders = input.Notifications?.ShouldEnableSignReminders ?? false ,
                    SignReminderFrequencyInDays = input.Notifications?.SignReminderFrequencyInDays ?? 1 ,
                    CanUserControlReminderSettings = input.Notifications?.CanUserControlReminderSettings ?? true,
                    SignerLinkExpirationInHours = input.Notifications?.SignerLinkExpirationInHours ?? 0,
                    EnableVisualIdentityFlow = input?.EnableVisualIdentityFlow ?? false,  
                    EnableDisplaySignerNameInSignature = input?.EnableDisplaySignerNameInSignature ?? false,
                    ShouldSendWithOTPByDefault = input?.ShouldSendWithOTPByDefault ?? false,
                    IsPersonzliedPFX = input?.IsPersonalizedPFX ?? false,
                    RecentPasswordsAmount = input?.RecentPasswordsAmount ?? 0,
                    PasswordExpirationInDays = input?.PasswordExpirationInDays ?? 0,
                    MinimumPasswordLength = input?.MinimumPasswordLength ?? 8,
                    ShouldSendDocumentNotifications = input?.Notifications.ShouldSendDocumentNotifications ?? false ,
                    DocumentNotificationsEndpoint = input?.Notifications?.DocumentNotificationsEndpoint ?? string.Empty,
                    ShouldAddAppendicesAttachmentsToSendMail = input?.ShouldAddAppendicesAttachmentsToSendMail ?? false,
                    DefaultSigningType = input?.DefaultSigningType ?? Common.Enums.PDF.SignatureFieldType.Graphic,
                    EnableTabletsSupport = input?.EnableTabletsSupport ?? false,
                    //ShouldEnableGovernmentSignatureFormat = input?.ShouldEnableGovernmentSignatureFormat ?? false,
                    DocumentDeletionConfiguration = new DocumentDeletionConfiguration
                    {
                        DeleteSignedDocumentAfterXDays = input.DeletionDetails?.DeleteSignedDocumentAfterXDays ?? 0,
                        DeleteUnsignedDocumentAfterXDays = input.DeletionDetails?.DeleteUnsignedDocumentAfterXDays ?? 0,
                    }
                },
                CompanySigner1Details = new CompanySigner1Details()
                {
                    CertId = input.CompanySigner1Details?.CertId,
                    CertPassword = input.CompanySigner1Details?.CertPassword,
                    ShouldShowInUI = input.CompanySigner1Details?.ShouldShowInUI ?? false,
                    ShouldSignAsDefaultValue = input.CompanySigner1Details?.ShouldSignAsDefaultValue ?? false,
                    Signer1Configuration = new Signer1Configuration()
                    {
                        Endpoint = input.CompanySigner1Details?.Signer1Configuration.Endpoint,
                        User = input.CompanySigner1Details?.Signer1Configuration.User,
                        Password = input.CompanySigner1Details?.Signer1Configuration.Password,
                    }
                    
                },
                ProgramId = new Guid(input.ProgramId),                
                ProgramUtilization = new ProgramUtilization
                {
                    StartDate = _dater.UtcNow(),
                    LastResetDate = _dater.UtcNow(),
                    Expired = input.ExpirationTime
                }
            };
        }

        private ICollection<ActiveDirectoryGroup> GetADGroupsFromInput(CompanyDTO input)
        {
            ICollection<ActiveDirectoryGroup> activeDirectoryGroups = new List<ActiveDirectoryGroup>();
            foreach (var item in input.GroupsADMapper ?? new List<GroupsADMapperDTO>())
            {
                activeDirectoryGroups.Add(new ActiveDirectoryGroup()
                {
                    ActiveDirectoryContactsGroupName = item.ActiveDirectoryContactsGroupName,
                    GroupName = item.GroupName,
                    ActiveDirectoryUsersGroupName = item.ActiveDirectoryUsersGroupName
                }
                );
            }

            return activeDirectoryGroups;
        }

        #endregion
    }
}