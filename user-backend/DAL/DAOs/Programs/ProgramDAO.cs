namespace DAL.DAOs.Programs
{
    using Common.Models;
    using DAL.DAOs.Companies;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Programs")]
    public class ProgramDAO
    {
        [Key]
        public Guid Id { get; set; }
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
        public virtual ICollection<CompanyDAO> Companies { get; set; }
        public virtual ProgramUIViewDAO ProgramUIView { get; set; }

        public ProgramDAO() { }

        public ProgramDAO(Program program)
        {
            Id = program.Id == Guid.Empty ? default : program.Id; 
            Name = program.Name;
            Users = program.Users;
            Templates = program.Templates;
            DocumentsPerMonth = program.DocumentsPerMonth;
            SmsPerMonth = program.SmsPerMonth;
            VisualIdentificationsPerMonth = program.VisualIdentificationsPerMonth;
            VideoConferencePerMonth = program.VideoConferencePerMonth;
            ServerSignature = program.ServerSignature;
            SmartCard = program.SmartCard;
            Note = program.Note;
            ProgramUIView = new ProgramUIViewDAO(program.UIViewLicense);
            
        }
    }
}
