using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.DAL.DAOs.Templates;

namespace HistoryIntegratorService.DAL.Extensions
{
    public static class TemplateExtensions
    {
        public static Template ToDeletedDocumentTemplate(this TemplateDAO templateDAO)
        {
            if (templateDAO == null)
            {
                return null;
            }
            var template = new Template()
            {
                Id = templateDAO.Id,
                TemplateId = templateDAO.TemplateId,
                TemplateSignatureFields = templateDAO.TemplateSignatureFields.Select(tsf => tsf.ToDeletedTemplateSignatureField()).ToList()
            };
            return template;
        }

        public static TemplateSignatureField ToDeletedTemplateSignatureField(this TemplateSignatureFieldDAO signatureFieldDAO)
        {
            if (signatureFieldDAO == null)
            {
                return null;
            }

            var tsf = new TemplateSignatureField()
            {
                Id = signatureFieldDAO.Id,
                TemplateId = signatureFieldDAO.TemplateId,
                SignatureFieldType = signatureFieldDAO.SignatureFieldType,
                CompanyName = signatureFieldDAO.CompanyName
            };
            return tsf;
        }
    }
}
