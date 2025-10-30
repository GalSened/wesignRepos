namespace Common.Interfaces.DB
{
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISignerTokenMappingConnector
    {
        Task Create(SignerTokenMapping signerTokenMapping);

        Task<SignerTokenMapping> Read(SignerTokenMapping signerTokenMapping);

        Task Delete(SignerTokenMapping signerTokenMapping);
        
      

        Task Update(SignerTokenMapping signerTokenMapping);
    }
}
