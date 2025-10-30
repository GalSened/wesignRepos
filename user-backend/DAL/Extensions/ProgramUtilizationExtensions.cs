namespace DAL.Extensions
{
    using Common.Models.Programs;
    using DAL.DAOs.Programs;

    public static class ProgramUtilizationExtensions
    {
        public static ProgramUtilization ToProgramUtilization(this ProgramUtilizationDAO programUtilizationDAO)
        {
            return programUtilizationDAO == null ? null : new ProgramUtilization()
            {
                Id = programUtilizationDAO.Id,
                SMS = programUtilizationDAO.SMS,
                SmsSentNotifyCount = programUtilizationDAO.SmsSentNotifyCount,
                VisualIdentifications = programUtilizationDAO.VisualIdentifications,
                VisualIdentificationUsedNotifyCount = programUtilizationDAO.VisualIdentificationUsedNotifyCount,
                Templates = programUtilizationDAO.Templates,
                Users = programUtilizationDAO.Users,
                Expired = programUtilizationDAO.Expired,
                DocumentsUsage = programUtilizationDAO.DocumentsUsage,
                DocumentsSentNotifyCount = programUtilizationDAO.DocumentsSentNotifyCount,
                StartDate = programUtilizationDAO.StartDate,
                DocumentsLimit = programUtilizationDAO.DocumentsLimit,
                ProgramResetType = programUtilizationDAO.ProgramResetType,
                LastResetDate = programUtilizationDAO.LastResetDate
            };
        }

        public static ProgramUtilizationHistory ToProgramUtilizationHistory(this ProgramUtilizationHistoryDAO programUtilizationHistoryDAO)
        {
            return programUtilizationHistoryDAO == null ? null : new ProgramUtilizationHistory()
            {
                Id = programUtilizationHistoryDAO.Id,
                CompanyId = programUtilizationHistoryDAO.CompanyId,
                CompanyName = programUtilizationHistoryDAO.CompanyName,
                UpdateDate = programUtilizationHistoryDAO.UpdateDate,
                DocumentsUsage = programUtilizationHistoryDAO.DocumentsUsage,
                SmsUsage = programUtilizationHistoryDAO.SmsUsage,
                VisualIdentificationsUsage = programUtilizationHistoryDAO.VisualIdentificationsUsage,
                TemplatesUsage = programUtilizationHistoryDAO.TemplatesUsage,
                UsersUsage = programUtilizationHistoryDAO.UsersUsage,
                Expired = programUtilizationHistoryDAO.Expired,
                ResourceMode = programUtilizationHistoryDAO.ResourceMode
            };
        }
    }
}
