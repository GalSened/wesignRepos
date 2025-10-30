using Common.Models.License;

namespace WeSignManagement.Models.Programs
{
    public class ProgramDTO
    {
        public string Name { get; set; }
        public long Users { get; set; }
        public long Templates { get; set; }
        public long DocumentsPerMonth { get; set; }
        public long SmsPerMonth { get; set; }
        public long VisualIdentificationsPerMonth { get; set; }
        public long VideoConferencePerMonth { get; set; }
        
        public bool ServerSignature { get; set; }
        public bool SmartCard { get; set; }
        public string Note { get; set; }
        public UIViewLicense UIViewLicense { get; set; }

        public ProgramDTO()
        {

        }

        public ProgramDTO(Common.Models.Program program)
        {
            this.Name = program.Name;
            this.Users = program.Users;
            this.Templates = program.Templates;
            this.DocumentsPerMonth = program.DocumentsPerMonth;
            this.SmsPerMonth = program.SmsPerMonth;
            this.VisualIdentificationsPerMonth = program.VisualIdentificationsPerMonth;
            this.VideoConferencePerMonth = program.VideoConferencePerMonth;
            this.ServerSignature = program.ServerSignature;
            this.SmartCard = program.SmartCard;
            this.Note = program.Note;
            this.UIViewLicense = program.UIViewLicense;
        }
    }
}
