namespace WeSign.Models.Users.Responses
{
    using System;
    using Common.Enums.Program;
    using Common.Models.License;
    using Common.Models.Programs;

    public class ProgramResponseDTO
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public DateTime ExpiredTime { get; set; }
        public DateTime LastResetDate { get; set; }
        public long DocumentsForMonth { get; set; }
        public long RemainingDocumentsForMonth { get; set; }
        public long SMSLimit { get; set; }
        public long RemainingSMS { get; set; }
        public long VisualIdentificationsLimit { get; set; }
        public long VideoConferenceLimit { get; set; }
        
        public long RemainingVisualIdentifications { get; set; }
        public long RemainingVideoConference { get; set; }
        public long RemainingTemplates { get; set; }
        public long TemplatesLimit { get; set; }
        public long RemainingUsers { get; set; }
        public long UsersLimit { get; set; }
        public bool ShouldShowSelfSign { get; }
        public bool ShouldShowGroupSign { get; }
        public bool ShouldShowLiveMode { get; }
        public bool ShouldShowContacts { get; set; }
        public bool ServerSignature { get; set; }
        public bool SmartCard { get; set; }
        public UIViewLicense UIViewLicense { get; set; }
        public ProgramResetType ProgramResetType { get; }
        public bool IsSmsProviderSupportGloballySend { get; }

        public ProgramResponseDTO(ProfileProgram profileProgram)
        {
            Name = profileProgram?.Name;
            Note = profileProgram?.Note;
            ExpiredTime = profileProgram?.Expired ?? DateTime.MinValue;
            LastResetDate = profileProgram?.LastResetDate ?? DateTime.MinValue;
            RemainingDocumentsForMonth = profileProgram?.RemainingDocuments ?? 0;
            DocumentsForMonth = profileProgram?.DocumentsLimits ?? 0;
            RemainingSMS = profileProgram?.RemainingSMS ?? 0;
            SMSLimit = profileProgram?.SMSLimit ?? 0;
            RemainingVisualIdentifications = profileProgram?.RemainingVisualIdentifications ?? 0;
            RemainingVideoConference = profileProgram?.RemainingVideoConference ?? 0;
            VisualIdentificationsLimit = profileProgram?.VisualIdentificationsLimit ?? 0;
            VideoConferenceLimit =  profileProgram?.VideoConferenceLimit ?? 0;
            RemainingTemplates = profileProgram?.RemainingTemplates ?? 0;
            TemplatesLimit = profileProgram?.TemplatesLimit ?? 0;
            UsersLimit = profileProgram?.UsersLimit ?? 0;
            RemainingUsers = profileProgram?.RemainingUsers ?? 0;
            UIViewLicense = profileProgram.ViewLicense;
            ServerSignature = profileProgram?.ServerSignature ?? false;
            SmartCard = profileProgram?.SmartCard ?? false;
            ProgramResetType = profileProgram.ProgramResetType;
            IsSmsProviderSupportGloballySend = profileProgram?.IsSmsProviderSupportGloballySend ?? false;
        }
    }
}
