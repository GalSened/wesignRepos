using Common.Enums.Program;
using Common.Models.Programs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Programs
{
    [Table("ProgramUtilizationHistories")]
    public class ProgramUtilizationHistoryDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime UpdateDate { get; set; }
        public long DocumentsUsage { get; set; }
        public long SmsUsage { get; set; }
        public long VisualIdentificationsUsage { get; set; }
        public long TemplatesUsage { get; set; }
        public long UsersUsage { get; set; }
        public DateTime Expired { get; set; }        
        public ProgramUtilizationHistoryResourceMode ResourceMode { get; set; }

        public ProgramUtilizationHistoryDAO() { }
        public ProgramUtilizationHistoryDAO(ProgramUtilizationHistory programUtilizationHistory)
        {
            Id = programUtilizationHistory.Id;
            CompanyId = programUtilizationHistory.CompanyId;
            CompanyName = programUtilizationHistory.CompanyName;
            UpdateDate = programUtilizationHistory.UpdateDate;
            DocumentsUsage = programUtilizationHistory.DocumentsUsage;
            SmsUsage = programUtilizationHistory.SmsUsage;
            VisualIdentificationsUsage = programUtilizationHistory.VisualIdentificationsUsage;
            TemplatesUsage = programUtilizationHistory.TemplatesUsage;
            UsersUsage = programUtilizationHistory.UsersUsage;
            Expired = programUtilizationHistory.Expired;
            ResourceMode = programUtilizationHistory.ResourceMode;
        }
    }    
}
