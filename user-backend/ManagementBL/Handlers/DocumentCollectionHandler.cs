using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class DocumentCollectionHandler : IDocumentCollection
    {
        private readonly ICompanyConnector _companyConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;

        public DocumentCollectionHandler(ICompanyConnector companyConnector, IDocumentCollectionConnector documentCollectionConnector)
        {
            _companyConnector = companyConnector;
            _documentCollectionConnector = documentCollectionConnector;
        }

        public async Task<IEnumerable<DocumentCollection>> ReadByStatusAndDate(Company company, DateTime notifyWhatBeforeDate, bool readCompany)
        {
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            Company dbCompany = null;
            if (readCompany)
                dbCompany = await _companyConnector.Read(company);
            else
                dbCompany = company;

            if (dbCompany == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            if (notifyWhatBeforeDate >  DateTime.UtcNow)
            {
                throw new InvalidOperationException(ResultCode.InvalidDateTime.GetNumericString());
            }

            var dc = _documentCollectionConnector.ReadByStatusAndDate(dbCompany, notifyWhatBeforeDate);
            return dc;
        }
    }

    public interface IDocumentCollection
    {
       Task<IEnumerable<DocumentCollection>> ReadByStatusAndDate(Company company, DateTime notifyWhatBeforeDate, bool readCompany);
    }
}
