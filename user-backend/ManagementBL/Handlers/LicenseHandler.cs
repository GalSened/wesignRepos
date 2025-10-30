using Common.Enums.License;
using Common.Interfaces.ManagementApp;
using Common.Models.ManagementApp;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Models.License;
using Common.Handlers;
using Common.Enums.Results;
using Common.Models;
using System.Collections.Generic;
using Common.Interfaces.DB;
using Common.Interfaces;
using Newtonsoft.Json;
using Common.Consts;
using System.IO.Abstractions;
using Common.Enums.Companies;
using Common.Interfaces.License;

namespace ManagementBL.Handlers
{
    public class LicenseHandler : BaseLicenseHandler, ILicense
    {
        private readonly ILicenseDMZ _licenseDMZ;
        private readonly ICompanyConnector _companyConnector;
        private readonly IProgramConnector _programConnector;


        public LicenseHandler(ICompanyConnector companyConnector,IProgramConnector programConnector,  ILogger logger, ILicenseDMZ licenseDMZ, IOptions<GeneralSettings> generalSettings, IOptions<FolderSettings> folderSettings,  ILicenseWrapper licenseWrapper, IFileSystem fileSystem)
         : base(generalSettings, folderSettings, logger, licenseWrapper, fileSystem)
        {
            _logger = logger;
            _licenseDMZ = licenseDMZ;
            _companyConnector = companyConnector;
            _programConnector = programConnector;
        }

        public async Task<GenerateLicenseKeyResponse> GenerateLicense(UserInfo userInfo)
        {
            Comda.License.Models.UserInfo info = new Comda.License.Models.UserInfo
            {
                Company = userInfo.Company,
                Email = userInfo.Email,
                Id = userInfo.Id,
                Name = userInfo.Name,
                Phone = userInfo.Phone
            };
            _logger.Debug("Generate License for : {@Info} ", info) ;
            bool isDMZReachable = await _licenseDMZ.IsDMZReachable();
            _logger.Debug("Generate License | IsDMZReachable = {IsDMZReachable} ", isDMZReachable); 
            var licenseResponse = await _licenseManager.Init(info, isDMZReachable);
            _logger.Debug("Generate License | LicenseManager Init response = Status : {LicenseStatusDescription} , HashedLicenseRequest : {HashedLicenseRequest} ", licenseResponse.Status.GetDescription(), licenseResponse.HashedLicenseRequest) ;

            if (licenseResponse?.Status != Comda.License.Enums.LicenseStatus.Init)
            {
                throw new InvalidOperationException(ResultCode.FailedInitLicenseRequest.GetNumericString());
            }

            return new GenerateLicenseKeyResponse
            {
                LicenseStatus = isDMZReachable ? LicenseStatus.SentToDMZ : LicenseStatus.Succsess,
                License = licenseResponse?.HashedLicenseRequest
            };
        }

        public LicenseStatus ActivateLicense(string license)
        {
            Comda.License.Enums.LicenseStatus licenseStatus = _licenseManager.Activate(license);
            if (licenseStatus == Comda.License.Enums.LicenseStatus.Activated)
            {
                return LicenseStatus.Succsess;
            }

            return LicenseStatus.Failed;
        }

        public async Task<(IWeSignLicense, LicenseCounters licenseUsage) > GetLicenseInformationAndUsing( bool readUsage)
        {
            LicenseCounters licenseUsage;
            IWeSignLicense weSignLicense = ReadLicenseInformation(); 
            if (readUsage)
            {
                var companies = _companyConnector.Read(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out int totalCount);
                var allCompaniesProgramSum = await GetAllCompaniesProgramSum(companies);
                licenseUsage = new LicenseCounters
                {
                    DocumentsPerMonth = weSignLicense.LicenseCounters.DocumentsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED :  allCompaniesProgramSum.DocumentsPerMonth,
                    Templates = weSignLicense.LicenseCounters.Templates == Consts.UNLIMITED ? Consts.UNLIMITED : allCompaniesProgramSum.Templates,
                    SmsPerMonth = weSignLicense.LicenseCounters.SmsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : allCompaniesProgramSum.SmsPerMonth,
                    VisualIdentificationsPerMonth = weSignLicense.LicenseCounters.VisualIdentificationsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : allCompaniesProgramSum.VisualIdentificationsPerMonth,
                    Users = weSignLicense.LicenseCounters.Users == Consts.UNLIMITED ? Consts.UNLIMITED : allCompaniesProgramSum.Users
                };
            }
            else
            {
                licenseUsage = new LicenseCounters();
            }
            return ( weSignLicense, licenseUsage);
        }

        public async Task ValidateProgramAddition(Program companyProgram, IEnumerable<Company> companies)
        {
            companyProgram =await _programConnector.Read(companyProgram);
            if (companyProgram == null)
            {
                throw new InvalidOperationException(ResultCode.ProgramNotExist.GetNumericString());
            }
            Program allCompaniesProgramSum = await GetAllCompaniesProgramSum(companies);

            var licenseInfo = ReadLicenseInformation();
            if ((licenseInfo.LicenseCounters.DocumentsPerMonth != Consts.UNLIMITED &&
                (licenseInfo.LicenseCounters.DocumentsPerMonth - 
                allCompaniesProgramSum.DocumentsPerMonth < companyProgram.DocumentsPerMonth)) 
                || (licenseInfo.LicenseCounters.DocumentsPerMonth != Consts.UNLIMITED && companyProgram.DocumentsPerMonth == Consts.UNLIMITED))
            {
                throw new InvalidOperationException(ResultCode.DocumentsExceedLicenseLimit.GetNumericString());
            }
            if ((licenseInfo.LicenseCounters.Templates != Consts.UNLIMITED &&
                (licenseInfo.LicenseCounters.Templates - allCompaniesProgramSum.Templates < companyProgram.Templates)) 
                || (licenseInfo.LicenseCounters.Templates != Consts.UNLIMITED && companyProgram.Templates == Consts.UNLIMITED))
            {
                throw new InvalidOperationException(ResultCode.TemplatesExceedLicenseLimit.GetNumericString());
            }
            if ((licenseInfo.LicenseCounters.Users != Consts.UNLIMITED &&
                (licenseInfo.LicenseCounters.Users - allCompaniesProgramSum.Users < companyProgram.Users)) 
                || (licenseInfo.LicenseCounters.Users != Consts.UNLIMITED && companyProgram.Users == Consts.UNLIMITED))
            {
                throw new InvalidOperationException(ResultCode.UsersExceedLicenseLimit.GetNumericString());
            }
            if ((licenseInfo.LicenseCounters.SmsPerMonth != Consts.UNLIMITED &&
                (licenseInfo.LicenseCounters.SmsPerMonth - allCompaniesProgramSum.SmsPerMonth < companyProgram.SmsPerMonth))
                || (licenseInfo.LicenseCounters.SmsPerMonth != Consts.UNLIMITED && companyProgram.SmsPerMonth == Consts.UNLIMITED))
            {
                throw new InvalidOperationException(ResultCode.SmsExceedLicenseLimit.GetNumericString());
            }
            if (companyProgram.VisualIdentificationsPerMonth == 0)
                return;
            if ((licenseInfo.LicenseCounters.VisualIdentificationsPerMonth != Consts.UNLIMITED &&
              (licenseInfo.LicenseCounters.VisualIdentificationsPerMonth - allCompaniesProgramSum.VisualIdentificationsPerMonth < companyProgram.VisualIdentificationsPerMonth))
              || (licenseInfo.LicenseCounters.VisualIdentificationsPerMonth != Consts.UNLIMITED && companyProgram.VisualIdentificationsPerMonth == Consts.UNLIMITED))
            {
                throw new InvalidOperationException(ResultCode.VisualIdentificationsExceedLicenseLimit.GetNumericString());
            }

        }

        private async Task<Program> GetAllCompaniesProgramSum(IEnumerable<Company> allCompanies)
        {
            var result = new Program();
            
            foreach (var comp in allCompanies)
            {
                var program = await _programConnector.Read(new Program { Id = comp.ProgramId });

                if(program.DocumentsPerMonth == Consts.UNLIMITED || result.DocumentsPerMonth == Consts.UNLIMITED)
                {
                    result.DocumentsPerMonth = Consts.UNLIMITED;
                }
                else
                {
                    result.DocumentsPerMonth += program.DocumentsPerMonth;
                }
                
                if (program.Users == Consts.UNLIMITED || result.Users == Consts.UNLIMITED)
                {
                    result.Users = Consts.UNLIMITED;
                }
                else
                {
                    result.Users += program.Users;
                }

                if (program.Templates == Consts.UNLIMITED || result.Templates == Consts.UNLIMITED)
                {
                    result.Templates = Consts.UNLIMITED;
                }
                else
                {
                    result.Templates += program.Templates;
                }

                if (program.SmsPerMonth == Consts.UNLIMITED || result.SmsPerMonth == Consts.UNLIMITED)
                {
                    result.SmsPerMonth = Consts.UNLIMITED;
                }
                else
                {
                    result.SmsPerMonth += program.SmsPerMonth;
                }
                if (program.VisualIdentificationsPerMonth == Consts.UNLIMITED || result.VisualIdentificationsPerMonth == Consts.UNLIMITED)
                {
                    result.VisualIdentificationsPerMonth = Consts.UNLIMITED;
                }
                else
                {
                    result.VisualIdentificationsPerMonth += program.VisualIdentificationsPerMonth;
                }
            } 


            return result;
        }
    }
}
