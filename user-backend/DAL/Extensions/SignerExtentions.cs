using Common.Enums.Logs;
using Common.Models;
using Common.Models.Documents.Signers;
using DAL.DAOs.Documents;
using DAL.DAOs.Documents.Signers;
using DAL.DAOs.Logs;
using System;
using System.Linq;

namespace DAL.Extensions
{
    public static class SignerExtentions
    {
        public static Signer ToSigner(this SignerDAO signerDAO)
        {
            if (signerDAO == null)
            {
                return null;
            }
            var signer = new Signer()
            {
                Id = signerDAO.Id,
                SendingMethod = signerDAO.SendingMethod,
                Status = signerDAO.Status,
                TimeSent = signerDAO.TimeSent,
                TimeViewed = signerDAO.TimeViewed,
                TimeSigned = signerDAO.TimeSigned,
                TimeRejected = signerDAO.TimeRejected,
                TimeLastSent = signerDAO.TimeLastSent,
                Notes = ToSignerNotes(signerDAO.Notes),
                SignerAuthentication = ToSignerAuthentication(signerDAO.OtpDetails, signerDAO.AuthMode),
                IdentificationAttempts = signerDAO.IdentificationAttempts,
                Contact = signerDAO.Contact.ToContact(),
                IPAddress = signerDAO.IPAddress,
                DeviceInformation = signerDAO.DeviceInformation,
                FirstViewIPAddress = signerDAO.FirstViewIPAddress,
            };
            if (signerDAO.SignerFields != null)
            {
                signer.SignerFields = signerDAO.SignerFields.Select(f => ToSignerField(f)).ToList();
            }
            if (signerDAO.SignerAttachments != null)
            {
                signer.SignerAttachments = signerDAO.SignerAttachments.Select(f => ToSignerAttachment(f)).ToList();
            }

            return signer;

        }
        private static SignerAuthentication ToSignerAuthentication(SignerOtpDetailsDAO otpDetailsDAO, AuthMode authMode)
        {
            return new SignerAuthentication
            {
                AuthenticationMode = authMode,
                OtpDetails = otpDetailsDAO == null ? null: new OtpDetails
                {
                    Identification = otpDetailsDAO.Identification,
                    Code = otpDetailsDAO.Code,
                    ExpirationTime = otpDetailsDAO.ExpirationTime,
                    Mode = otpDetailsDAO.Mode,
                    Attempts = otpDetailsDAO.Attempts
                }
            };
        }

        private static SignerAttachment ToSignerAttachment(SignerAttachmentDAO signerAttachmentDAO)
        {
            return signerAttachmentDAO == null ? null : new SignerAttachment()
            {
                Id = signerAttachmentDAO.Id,
                IsMandatory = signerAttachmentDAO.IsMandatory,
                Name = signerAttachmentDAO.Name
            };
        }
        private static Notes ToSignerNotes(NotesDAO notesDAO)
        {
            return notesDAO == null ? null : new Notes()
            {
                Id = notesDAO.Id,
                UserNote = notesDAO.UserNote,
                SignerNote = notesDAO.SignerNote
            };

        }

        private static SignerField ToSignerField(SignerFieldDAO signerFieldDAO)
        {
            return signerFieldDAO == null ? null : new SignerField()
            {
                Id = signerFieldDAO.Id,
                DocumentId = signerFieldDAO.DocumentId,
                FieldName = signerFieldDAO.FieldName
            };
        }

    }
}
