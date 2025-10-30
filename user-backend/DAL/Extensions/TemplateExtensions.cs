namespace DAL.Extensions
{
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Files.PDF;
    using DAL.DAOs.Templates;
    using System.Linq;
    using System.Net.Http.Headers;

    public static class TemplateExtensions
    {

        public static Template ToTemplate(this TemplateDAO templateDAO)
        {
            if (templateDAO == null)
            {
                return null;
            }
            Template template = new Template()
            {
                Id = templateDAO.Id,
                UserId = templateDAO.UserId,
                GroupId = templateDAO.GroupId,
                Name = templateDAO.Name,
                CreationTime = templateDAO.CreationTime,
                LastUpdatetime = templateDAO.LastUpdatetime,
                UsedCount = templateDAO.UsedCount,
                LastUsedTime= templateDAO.LastUsedTime,
                Status = templateDAO.Status
            };
            if (templateDAO.TemplateSignatureFields != null)
            {
                foreach (TemplateSignatureFieldDAO field in templateDAO.TemplateSignatureFields)
                {
                    template.Fields.SignatureFields.Add(ToSignatureFields(field));
                }
            }
            if (templateDAO.TemplateTextFields != null)
            {
                foreach (TemplateTextFieldDAO field in templateDAO.TemplateTextFields)
                {
                    template.Fields.TextFields.Add(ToTextFields(field));
                }
            }

            return template;
        }

        public static DeletedDocumentTemplate ToDeletedDocumentTemplate(this TemplateDAO templateDAO)
        {
            if (templateDAO == null)
            {
                return null;
            }
            DeletedDocumentTemplate deletedTemplate = new DeletedDocumentTemplate();
            if (templateDAO.TemplateSignatureFields != null)
            {
                foreach (var field in templateDAO.TemplateSignatureFields)
                {
                    deletedTemplate.TemplateSignatureFields.Add(ToDeletedTemplateSignatureField(field));
                }
            }
            return deletedTemplate;
        }

        public static DeletedTemplateSignatureField ToDeletedTemplateSignatureField(this TemplateSignatureFieldDAO signatureFieldDAO)
        {
            return signatureFieldDAO == null ? null : new DeletedTemplateSignatureField()
            {
                SignatureFieldType = signatureFieldDAO.SignaturFieldType,
                CompanyName = signatureFieldDAO.Template.Documents.FirstOrDefault().DocumentCollection.User.Company.Name
            };
        }

        public static SignatureField ToSignatureFields(this TemplateSignatureFieldDAO signatureFieldDAO)
        {
            return signatureFieldDAO == null ? null : new SignatureField()
            {
                SigningType = signatureFieldDAO.SignaturFieldType,
                SignatureKind = signatureFieldDAO.SignatureKind,
                Name = signatureFieldDAO.Name,
                Mandatory = signatureFieldDAO.Mandatory
            };
        }

        public static TextField ToTextFields(this TemplateTextFieldDAO textFieldDAO)
        {
            return textFieldDAO == null ? null : new TextField()
            {
                TextFieldType = textFieldDAO.TextFieldType,
                Name = textFieldDAO.Name,
                CustomerRegex = textFieldDAO.Regex
            };
        }


        public static SingleLinkAdditionalResource ToSingleLinkAdditionalResource(this SingleLinkAdditionalResourceDAO singleLinkAdditionalResourceDAO)
        {
            return singleLinkAdditionalResourceDAO == null ? null : new SingleLinkAdditionalResource()
            {
                
                Data = singleLinkAdditionalResourceDAO.Data,
                
                Type = singleLinkAdditionalResourceDAO.Type,
                IsMandatory = singleLinkAdditionalResourceDAO.IsMandatory,
                TemplateId = singleLinkAdditionalResourceDAO.TemplateId


            };
        }

    }
}
