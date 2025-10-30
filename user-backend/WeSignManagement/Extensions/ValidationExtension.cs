using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WeSignManagement.Models.Companies;
using WeSignManagement.Models.Configurations;
using WeSignManagement.Models.License;
using WeSignManagement.Models.Payment;
using WeSignManagement.Models.Programs;
using WeSignManagement.Models.Users;
using WeSignManagement.Validators.Companies;
using WeSignManagement.Validators.Configuration;
using WeSignManagement.Validators.License;
using WeSignManagement.Validators.Payment;
using WeSignManagement.Validators.ProgramValidators;
using WeSignManagement.Validators.UserValidators;

namespace WeSignManagement.Extensions
{
    public static class ValidationExtension
    {
        public static void AddValidation(this IServiceCollection services)
        {
            services.AddTransient<IValidator<LoginManagementRequestDTO>, LoginRequestValidator>();
            services.AddTransient<IValidator<ResetPasswordRequestDTO>, ResetPasswordRequestValidator>();
            services.AddTransient<IValidator<TokensManagementDTO>, TokensValidator>();
            services.AddTransient<IValidator<ProgramDTO>, ProgramValidator>();
            services.AddTransient<IValidator<CompanyDTO>, CompanyValidator>();
            services.AddTransient<IValidator<ActivateLicenseDTO>, ActivateLicenseValidator>();
            services.AddTransient<IValidator<UserInfoDTO>, GenerateLicenseKeyValidator>();
            services.AddTransient<IValidator<CreateUserFromManagmentDTO>, CreateUserFromManagmentValidator>();
            services.AddTransient<IValidator<UpdateUserManagementDTO>, UpdateUserValidator>();
            services.AddTransient<IValidator<UserPaymentRequestDTO>, PaymentValidator>();
            services.AddTransient<IValidator<UpdateUserTypeDTO>, UpdateUserTypeValidator>();
            services.AddTransient<IValidator<SmtpDetailsDTO>, SmtpDetailsValidator>();
            services.AddTransient<IValidator<SmsDetailsDTO>, SmsDetailsValidator>();
            services.AddTransient<IValidator<CreateHtmlTemplateDTO>, CreateHtmlTemplateValidator>();
        }
    }
}
