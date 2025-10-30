using Common.Models;
using Common.Models.Documents;
using DAL.DAOs.Documents;
using System;
using System.Linq;

namespace DAL.Extensions
{
    public static class DocumentCollectionExtensions
    {
        public static DocumentCollection ToDocumentCollection(this DocumentCollectionDAO documentCollectionDAO)
        {
            if (documentCollectionDAO == null)
            {
                return null;
            }
            DocumentCollection documentGroup = new DocumentCollection()
            {
                Id = documentCollectionDAO.Id,
                UserId = documentCollectionDAO.UserId,
                GroupId = documentCollectionDAO.GroupId,
                DistributionId = documentCollectionDAO.DistributionId,
                Name = documentCollectionDAO.Name,
                RedirectUrl = documentCollectionDAO.RedirectUrl,
                CallbackUrl = documentCollectionDAO.CallbackUrl,
                DocumentStatus = documentCollectionDAO.Status,
                SignedTime = documentCollectionDAO.SignedTime,
                CreationTime = documentCollectionDAO.CreationTime,
                Mode = documentCollectionDAO.Mode,
                User = documentCollectionDAO.User.ToUser(),
                Notifications = new DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = documentCollectionDAO.ShouldSend,
                    ShouldSendSignedDocument = documentCollectionDAO.ShouldSendSignedDocument
                },
                ShouldSignUsingSigner1AfterDocumentSigningFlow = documentCollectionDAO.ShouldSignUsingSigner1AfterDocumentSigningFlow ?? false,
                ShouldEnableMeaningOfSignature = documentCollectionDAO.ShouldEnableMeaningOfSignature ,
                SenderIP = documentCollectionDAO.SenderIP,
            };

            if (documentCollectionDAO.Documents != null)
            {
                documentGroup.Documents = documentCollectionDAO.Documents.Select(d => ToDocument(d)).ToList();
            }
            if (documentCollectionDAO.Signers != null)
            {
                documentGroup.Signers = documentCollectionDAO.Signers.Select(s => s.ToSigner()).ToList();
            }

            return documentGroup;
        }

        public static DeletedDocumentCollection ToDeletedDocumentCollection(this DocumentCollectionDAO documentCollectionDAO)
        {
            if (documentCollectionDAO == null)
            {
                return null;
            }
            DeletedDocumentCollection deletedDocumentCollection = new DeletedDocumentCollection()
            {
                Id = documentCollectionDAO.Id,
                UserId = documentCollectionDAO.UserId,
                GroupId = documentCollectionDAO.GroupId,
                DistributionId = documentCollectionDAO.DistributionId,
                DocumentStatus = documentCollectionDAO.Status,
                CreationTime = documentCollectionDAO.CreationTime,
                User = new DeletedDocumentUser()
                {
                    CompanyId = documentCollectionDAO.User?.CompanyId ?? Guid.Empty,
                    Email = documentCollectionDAO.User?.Email,
                    Id = documentCollectionDAO.User?.Id ?? Guid.Empty,


                }
            };
            

            if (documentCollectionDAO.Documents != null)
            {
                deletedDocumentCollection.Documents = documentCollectionDAO.Documents.Select(d => ToDeletedDocument(d)).ToList();
            }

            return deletedDocumentCollection;
        }

        private static Document ToDocument(DocumentDAO documentDAO)
        {
            return documentDAO == null ? null : new Document()
            {
                Id = documentDAO.Id,
                Name = documentDAO.Name,
                TemplateId = documentDAO.TemplateId
            };
        }

        private static DeletedDocument ToDeletedDocument(DocumentDAO documentDAO)
        {
            if (documentDAO == null)
            {
                return null;
            }
            DeletedDocument deletedDocument = new DeletedDocument(documentDAO.ToDocument(), documentDAO.Template.ToDeletedDocumentTemplate());
            return deletedDocument;
        }
    }
}
