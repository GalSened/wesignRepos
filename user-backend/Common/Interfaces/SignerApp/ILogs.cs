using Common.Models;
using Common.Models.Documents.Signers;
using System.Threading.Tasks;

namespace Common.Interfaces.SignerApp
{
    public interface ILogs
    {
        Task Create(string token, LogMessage logMessage);
    }
}
