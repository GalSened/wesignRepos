using Common.Enums.Program;
using System;

namespace Common.Models.Programs
{
    public class ProgramUtilizationHistory
    {
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
    }
}
