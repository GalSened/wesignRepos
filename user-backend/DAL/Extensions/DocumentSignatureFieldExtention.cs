using Common.Models.Documents;
using DAL.DAOs.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Extensions
{
    public static class DocumentSignatureFieldExtention
    {
        public static DocumentSignatureField ToDocumentSignatureField(this DocumentSignatureFieldDAO documentSignatureFieldDAO)
        {
            return documentSignatureFieldDAO == null ? null : new DocumentSignatureField()
            {
                Id = documentSignatureFieldDAO.Id,
                FieldName = documentSignatureFieldDAO.FieldName,
                Image = documentSignatureFieldDAO.Image,
                DocumentId = documentSignatureFieldDAO.DocumentId
            };
        }
    }
}
