namespace DAL.DAOs.Documents.Signers
{
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using Common.Models.Documents.Signers;
    using DAL.DAOs.Contacts;
    using DAL.DAOs.Documents;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("Signers")]
    public class SignerDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentCollectionId { get; set; }
        public Guid ContactId { get; set; }
        public SignerStatus Status { get; set; }
        public DateTime? TimeSent { get; set; }
        public DateTime TimeViewed { get; set; }
        public DateTime TimeSigned { get; set; }
        public DateTime TimeRejected { get; set; }
        public DateTime TimeLastSent { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public AuthMode AuthMode { get; set; }
        public int IdentificationAttempts { get; set; }
        public string FirstViewIPAddress { get; set; }
        public string IPAddress { get; set; }

        public string DeviceInformation { get; set; }
        public virtual ICollection<SignerAttachmentDAO> SignerAttachments { get; set; }
        public virtual ICollection<SignerFieldDAO> SignerFields { get; set; }
        public virtual NotesDAO Notes { get; set; }
        public virtual SignerOtpDetailsDAO OtpDetails { get; set; }
        public virtual DocumentCollectionDAO DocumentCollection { get; set; }
        public virtual ContactDAO Contact { get; set; }


        public SignerDAO() { }
        public SignerDAO(Signer signer)
        {
            Id = signer.Id == Guid.Empty ? default : signer.Id;
            ContactId = signer.Contact?.Id == Guid.Empty ? default : signer.Contact.Id;
            Status = signer.Status;
            TimeSent = signer.TimeSent;
            TimeViewed = signer.TimeViewed;
            TimeSigned = signer.TimeSigned;
            TimeRejected = signer.TimeRejected;
            TimeLastSent = signer.TimeLastSent;
            SendingMethod = signer.SendingMethod;
            SignerAttachments = signer?.SignerAttachments.Select(s => new SignerAttachmentDAO(s)).ToList();
            SignerFields = signer?.SignerFields.Select(s => new SignerFieldDAO(s)).ToList();
            Notes = signer.Notes != null ? new NotesDAO(signer.Notes) : new NotesDAO();
            AuthMode = signer.SignerAuthentication?.AuthenticationMode ?? AuthMode.None;
            IdentificationAttempts = signer.IdentificationAttempts;
            IPAddress = signer.IPAddress;
            DeviceInformation = signer.DeviceInformation;
            FirstViewIPAddress = signer.FirstViewIPAddress;
            OtpDetails = signer?.SignerAuthentication?.OtpDetails != null ? new SignerOtpDetailsDAO(signer.SignerAuthentication.OtpDetails) : new SignerOtpDetailsDAO();
        }
    }
}
