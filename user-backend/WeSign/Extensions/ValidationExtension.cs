namespace WeSign.Extensions
{
    using FluentValidation;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using WeSign.Models.Admins;
    using WeSign.Models.Contacts;
    using WeSign.Models.Distribution.Requests;
    using WeSign.Models.Documents;
    using WeSign.Models.SelfSign;
    using WeSign.Models.Signers;
    using WeSign.Models.Templates;
    using WeSign.Models.Users;
    using WeSign.Validators.Admins;
    using WeSign.Validators.ContactValidators;
    using WeSign.Validators.Distribution;
    using WeSign.Validators.DocumentValidators;
    using WeSign.Validators.SelfSignValidators;
    using WeSign.Validators.Signers;
    using WeSign.Validators.TemplateValidators;
    using WeSign.Validators.UserValidators;

    public static class ValidationExtension
    {
        public static void AddValidation(this IServiceCollection services)
        {
            services.AddTransient<IValidator<CreateUserDTO>, CreateUserValidator>();
            services.AddTransient<IValidator<ChangePasswordDTO>, ChangePasswordValidator>();
            services.AddTransient<IValidator<LoginRequestDTO>, LoginValidator>();
            services.AddTransient<IValidator<RenewPasswordDTO>, RenewPasswordValidator>();
            services.AddTransient<IValidator<UpdateUserDTO>, UpdateUserValidator>();

            services.AddTransient<IValidator<AdminCreateUserDTO>, AdminAddUserValidator>();
            services.AddTransient<IValidator<AdminCreateGroupDTO>, AdminCreateGroupValidator>();

            services.AddTransient<IValidator<UpdateUserInGroupDTO>, UpdateUserInGroupValidator>();
            services.AddTransient<IValidator<ContactDTO>, ContactValidator>();
            services.AddTransient<IValidator<ActivationDTO>, ActivationValidator>();

            services.AddTransient<IValidator<CreateTemplateDTO>, CreateTemplateValidator>();
            services.AddTransient<IValidator<UpdateTemplateDTO>, UpdateTemplateValidator>();

            services.AddTransient<IValidator<CreateDocumentCollectionDTO>, CreateDocumentCollectionValidator>();
            services.AddTransient<IValidator<UpdateSelfSignDocumentDTO>, UpdateSelfSignDocumentValidator>();
            services.AddTransient<IValidator<Signer1FileSigingDTO>, Signer1FileSigingValidator>();
            services.AddTransient<IValidator<CreateSelfSignDocumentDTO>, CreateSelfSignDocumentValidator>();

            services.AddTransient<IValidator<DownloadBatchRequestDTO>, DocumentBatchDownloadRequestValidator>();
            services.AddTransient<IValidator<DeleteBatchRequestDTO>, DocumentBatchDeleteRequestValidator>();

            services.AddTransient<IValidator<CreateSimpleDocumentDTO>, CreateSimpleDocumentValidator>();

            services.AddTransient<IValidator<ContactsDTO>, ContactsValidator>();
            services.AddTransient<IValidator<CreateDistributionDocumentsDTO>, CreateDistributionDocumentsValidator>();
            services.AddTransient<IValidator<SignersForDistributionMechanismDTO>, SignersForDistributionMechanismValidator>();
            services.AddTransient<IValidator<ShareDTO>, ShareValidator>();
            services.AddTransient<IValidator<OtpDTO>, OtpValidator>();
            services.AddTransient<IValidator<OtpResendDTO>, OtpResendValidator>();
            services.AddTransient<IValidator<ReplaceSignerWithDetailsDTO>, ReplaceSignerValidator>();

        }

    }
}
