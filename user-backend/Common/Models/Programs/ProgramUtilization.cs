namespace Common.Models.Programs
{
    using Common.Enums.Program;
    using System;

    public class ProgramUtilization
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastResetDate { get; set; }
        public long Users { get; set; }
        public long Templates { get; set; }
        public long DocumentsUsage { get; set; }
        public long DocumentsLimit { get; set; }
        public int DocumentsSentNotifyCount { get; set; }
        public ProgramResetType ProgramResetType { get; set; }
        public long SMS { get; set; }
      
        public int SmsSentNotifyCount { get; set; }
        public long VisualIdentifications { get; set; }
        public int VisualIdentificationUsedNotifyCount { get; set; }
        public long VideoConference { get; set; }
        public int VideoConferenceUsedNotifyCount { get; set; }

        
        public DateTime Expired { get; set; }
    }
}

