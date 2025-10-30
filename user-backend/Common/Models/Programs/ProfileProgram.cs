namespace Common.Models.Programs
{
    using Common.Enums.Program;
    using Common.Models.License;
    using System;
    public class ProfileProgram
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public DateTime Expired { get; set; }
        public DateTime LastResetDate { get; set; }
        public long DocumentsLimits { get; set; }
        public long RemainingDocuments { get; set; }
        public long SMSLimit { get; set; }
        public long RemainingSMS { get; set; }
        public long VisualIdentificationsLimit { get; set; }
        public long VideoConferenceLimit { get; set; }
        public long RemainingVisualIdentifications { get; set;  }
        public long RemainingVideoConference { get; set; }
        
        public long TemplatesLimit { get; set; }
        public long RemainingTemplates { get; set; }
        public long UsersLimit { get; set; }
        public long RemainingUsers { get; set; }
        public bool ServerSignature { get; set; }
        public bool SmartCard { get; set; }
        public UIViewLicense ViewLicense { get; set; }
        public ProgramResetType ProgramResetType { get; set; }
        public bool IsSmsProviderSupportGloballySend { get; set; }

        public ProfileProgram()
        {
            ViewLicense = new UIViewLicense();
        }
    }
}
