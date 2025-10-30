using Common.Enums.Results;
using Common.Models;
using Common.Models.Configurations;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDoneActionsHelper
    {
         Task<ResultCode> HandlerSigningUsingSigner1AfterDocumentSigningFlow(DocumentCollection dbDcumentCollection, CompanySigner1Details companySigner1Details = null);
    }
}
