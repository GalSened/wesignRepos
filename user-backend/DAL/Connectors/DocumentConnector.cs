namespace DAL.Connectors
{
    using Common.Interfaces.DB;
    using Common.Models;
    using Common.Models.Documents;
    using DAL.DAOs.Documents;
    using DAL.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DocumentConnector : IDocumentConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;

        public DocumentConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Document> Read(Template template)
        {
            try
            {
                return (await _dbContext.Documents.FirstOrDefaultAsync(d => d.TemplateId == template.Id)).ToDocument();

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentConnector_ReadByTemplate = ");
                throw;
            }
        }

        public Task<bool> DocumentExist(Template template)
        {
            try
            {
                return _dbContext.Documents.AnyAsync(d => d.TemplateId == template.Id);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentConnector_DocumentExist = ");
                throw;
            }
        }


        public async Task<Document> Read(Document document)
        {
            try
            {
                return (await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == document.Id)).ToDocument();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentConnector_ReadByDocument = ");
                throw;
            }
        }

        public IEnumerable<DocumentSignatureField> ReadSignaturesByDocumentsId(List<Guid> documentsIds)
        {
            try
            {
                var documentSignatures = _dbContext.DocumentsSignatureFields.Where(x => documentsIds.Contains(x.DocumentId));

                return documentSignatures.Select(x => x.ToDocumentSignatureField()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentConnector_ReadSignaturesByDocumentsId = ");
                throw;
            }
        }


        public IEnumerable<DocumentSignatureField> ReadSignatures(Document document)
        {
            try
            {
                var documentSignatures = _dbContext.DocumentsSignatureFields.Where(x => x.DocumentId == document.Id);

                return documentSignatures.Select(x => x.ToDocumentSignatureField()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentConnector_ReadSignatures = ");
                throw;
            }
        }

        public IEnumerable<Document> ReadDocumentsById(List<Guid> guids)
        {
            try
            {
                return _dbContext.Documents.Where(d => guids.Contains(d.Id)).Select(x => x.ToDocument());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentConnector_ReadDocumentsById = ");
                throw;
            }

        }
    }
}
