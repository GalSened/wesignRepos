using Common.Enums;
using Common.Models.Documents.Signers;
using System;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISigners
    {
        Task ReplaceSigner(Guid id, Guid signerId, string name, string means, Notes notes, OtpMode otpMode, string otpPassword, AuthMode authMode);
    }
}