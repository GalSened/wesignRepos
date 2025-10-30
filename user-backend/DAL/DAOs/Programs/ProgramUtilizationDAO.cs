namespace DAL.DAOs.Programs
{
    using Common.Enums.Program;
    using Common.Models.Programs;
    using DAL.DAOs.Companies;
    using DAL.DAOs.Users;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ProgramUtilizations")]
    public class ProgramUtilizationDAO
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastResetDate { get; set; }
        public long Users { get; set; }
        public long Templates { get; set; }
        public ProgramResetType ProgramResetType { get; set; }
        public long DocumentsUsage { get; set; }        
        public long DocumentsLimit { get; set; }
        public int DocumentsSentNotifyCount { get; internal set; }
        public long SMS { get; set; }
      
        public int SmsSentNotifyCount { get; internal set; }
        public long VisualIdentifications { get; set; }
        public long VideoConference { get; set; }
        public int VisualIdentificationUsedNotifyCount { get;  set; }
        public int VideoConferenceUsedNotifyCount { get;  set; }

       
        public DateTime Expired { get; set; }
        public virtual UserDAO User { get; set; }
        public virtual CompanyDAO Company { get; set; }

        public ProgramUtilizationDAO() { }

        public ProgramUtilizationDAO(ProgramUtilization programUtilization)
        {
            Users = programUtilization.Users;
            Templates = programUtilization.Templates;
            DocumentsUsage = programUtilization.DocumentsUsage;
            SMS = programUtilization.SMS;
            VisualIdentifications = programUtilization.VisualIdentifications;
            Expired = programUtilization.Expired;
            DocumentsLimit = programUtilization.DocumentsLimit;
            ProgramResetType = programUtilization.ProgramResetType;
            Id = programUtilization.Id;
            StartDate = programUtilization.StartDate;
            LastResetDate = programUtilization.LastResetDate;
        }
    }
}
