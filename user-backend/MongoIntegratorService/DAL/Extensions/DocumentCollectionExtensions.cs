using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.DAL.DAOs.Documents;

namespace HistoryIntegratorService.DAL.Extensions
{
    public static class DocumentCollectionExtensions
    {
        public static DeletedDocumentCollection ToDeletedDocumentCollection(this DeletedDocumentCollectionDAO documentCollectionDAO)
        {
            if (documentCollectionDAO == null)
            {
                return null;
            }

            DeletedDocumentCollection deletedDocCollection = new DeletedDocumentCollection()
            {
                Id = documentCollectionDAO.Id,
                UserId = documentCollectionDAO.UserId,
                GroupId = documentCollectionDAO.GroupId,
                DistributionId = documentCollectionDAO.DistributionId,
                DocumentStatus = documentCollectionDAO.DocumentStatus,
                CreationTime = documentCollectionDAO.CreationTime,
                User =new User()
                {
                    CompanyId = documentCollectionDAO.CompanyId,
                    CompanyName = documentCollectionDAO.CompanyName,
                    Email = documentCollectionDAO.UserEmail
                }


                
            };

            if (documentCollectionDAO.Documents != null)
            {
                deletedDocCollection.Documents = documentCollectionDAO.Documents.Select(d => d.ToDocument()).ToList();
            }

            return deletedDocCollection;
        }

        public static DeletedDocument ToDocument(this DeletedDocumentDAO documentDAO)
        {
            if (documentDAO == null)
            {
                return null;
            }

            var doc = new DeletedDocument()
            {
                Id = documentDAO.Id,
                Template = documentDAO.Template.ToDeletedDocumentTemplate()
            };

            return doc;
        }
    }
}
