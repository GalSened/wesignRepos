using System.Threading.Tasks;

namespace ManagementBL.CleanDb
{
    public interface IDeleter
    {       
        Task<bool> DeleteProcess();
    }
}
