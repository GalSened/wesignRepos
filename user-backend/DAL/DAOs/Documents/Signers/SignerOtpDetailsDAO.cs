using Common.Enums;
using Common.Models.Documents.Signers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Documents.Signers
{
    [Table("SignerOtpDetails")]
    public class SignerOtpDetailsDAO
    {
        public Guid Id { get; set; }
        public Guid SignerId { get; set; }
        public string Identification { get; set; }
        public int Attempts { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public OtpMode Mode { get; set; }
        public virtual SignerDAO Signer { get; set; }

        public SignerOtpDetailsDAO() { }
        public SignerOtpDetailsDAO(OtpDetails otpDetails)
        {
            if (otpDetails != null)
            {
                Code = otpDetails.Code;
                ExpirationTime= otpDetails.ExpirationTime;
                Identification = otpDetails.Identification;
                Mode = otpDetails.Mode;
            }
        }
    }
}