using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WeSignSigner.Models.Requests;
using WeSignSigner.Models.Responses;
using WeSignSigner.Validators;

namespace WeSignSigner.Extensions
{
    public static class ValidationExtension
    {
        public static void AddValidation(this IServiceCollection services)
        {
            services.AddTransient<IValidator<UpdateDocumentCollectionDTO>, UpdateDocumentCollectionValidator>();
            services.AddTransient<IValidator<CreateDocumentDTO>, CreateSingleLinkDocuementValidator>();
            services.AddTransient<IValidator<SignaturesImagesDTO>, SignaturesImagesVaidator>();
        }
    }
}
