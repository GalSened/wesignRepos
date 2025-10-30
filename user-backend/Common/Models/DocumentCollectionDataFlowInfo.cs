using Common.Enums;
using Common.Enums.Users;
using Common.Models.Documents.Signers;
using System;

namespace Common.Models
{
    public class DocumentCollectionDataFlowInfo 
    {
        public Guid MapperID { get; set; }
        public OtpMode OtpMode { get; set; }
        public Language Language { get; set; }
        public string CompanyLogo { get; set; }
        public string Means { get; set; }
        public string Name { get; set; }
        public bool VisualIdentificationRequired { get; set; }
        public bool ShouldDisplaySignerNameInSignature { get; set; }
        public bool ShouldDisplayMeaningOfSignature { get; set; }
        public bool ShouldSignEidasSignatureFlow { get; set; }
    }
}