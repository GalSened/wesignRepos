namespace WeSign.Models.Documents
{
    using Common.Enums.Documents;
    using Common.Models.Documents;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CreateDocumentCollectionDTO
    {
        public DocumentMode DocumentMode { get; set; }
        public string DocumentName { get; set; }
        public Guid[] Templates { get; set; }
        public string SenderNote { get; set; }
        public IEnumerable<SignerDTO> Signers { get; set; }
        public IEnumerable<SignerFieldDTO> ReadOnlyFields { get; set; }
        public string RediretUrl { get; set; }
        public string CallBackUrl { get; set; }
        public IEnumerable<Appendix> SenderAppendices { get; set; }
        public IEnumerable<SharedAppendixDTO> SharedAppendices { get; set; }
        public DocumentNotificationsDTO Notifications { get; set; }
        public bool ShouldSignUsingSigner1AfterDocumentSigningFlow { get; set; }
        public bool ShouldEnableMeaningOfSignature { get; set; }
        public CreateDocumentCollectionDTO()
        {
            Signers = new List<SignerDTO>();
        }

    }
}