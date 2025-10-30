// Ignore Spelling: App

using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface IJobs
    {        
        Task CreateActiveDirectoryUsersAndContacts();
        Task CleanDb();
        //void DeleteDocumentsFromServer();
        /// <summary>
        /// Send alert to users 30 and 15 days and every day in the last week before they program expired
        /// </summary>
        Task SendProgramExpiredNotification();
        Task ResetProgramsUtilization();
      
        Task DeleteLogsFromDB();
        Task<int> SendDocumentIsAboutToBeDeletedNotification();
        Task SendProgramCapacityIsAboutToExpiredNotification();
        Task CleanUnusedTemplatesAndContacts();
        Task SendSignReminders();
        Task SendUserPeriodicReports();
        Task SendManagementPeriodicReports();
        Task DeleteExpiredPeriodicReportFiles();
        Task UpdateExpiredSignerTokens();
    }
}
