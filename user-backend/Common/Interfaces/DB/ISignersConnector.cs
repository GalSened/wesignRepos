
using Common.Enums.Contacts;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface ISignersConnector
    {
        Task<Signer> GetSignerById(Guid id);
        IEnumerable<Signer> ReadSignersByEmail(string email);
        IEnumerable<Signer> ReadPendingSigners(Guid documentCollectionId);
        Task UpdateGeneratedOtpDetails(Signer dbSigner);
        Task UpdateOtpAttempts(Guid signerId, int attempts);
        Task UpdateIdentificationAttempts(Signer signer);
        Task UpdateSignerStatus(Signer signer);
        Task UpdateSignerNotes(Guid signerId, string notes);
    }
}