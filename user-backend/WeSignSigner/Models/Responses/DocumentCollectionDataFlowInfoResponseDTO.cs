using Common.Enums;
using Common.Enums.Users;
using Common.Models;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignSigner.Models.Responses
{
    public class DocumentCollectionDataFlowInfoResponseDTO
    {
        public DocumentCollectionDataFlowInfoResponseDTO(DocumentCollectionDataFlowInfo documentCollectionDataFlowInfo)
        {
            MapperID = documentCollectionDataFlowInfo?.MapperID?? default ;
            Language = documentCollectionDataFlowInfo?.Language ??  Language.en;
            OtpMode = documentCollectionDataFlowInfo?.OtpMode ?? OtpMode.None;
            CompanyLogo = documentCollectionDataFlowInfo?.CompanyLogo;
            Means = documentCollectionDataFlowInfo?.Means;
            Name = documentCollectionDataFlowInfo?.Name;
            VisualIdentificationRequired = documentCollectionDataFlowInfo.VisualIdentificationRequired;
            ShouldDisplaySignerNameInSignature = documentCollectionDataFlowInfo.ShouldDisplaySignerNameInSignature;
            ShouldDisplayMeaningOfSignature = documentCollectionDataFlowInfo.ShouldDisplayMeaningOfSignature;
            ShouldSignEidasSignatureFlow = documentCollectionDataFlowInfo.ShouldSignEidasSignatureFlow;
        }

        public Guid MapperID { get; set; }       
        public OtpMode OtpMode { get; set; }
        public Language Language { get; set; }
        public string CompanyLogo { get; set; }
        public string Means { get; set; }
        public string Name { get; set; }
        public bool VisualIdentificationRequired { get; }
        public bool ShouldDisplaySignerNameInSignature { get; set; }
        public bool ShouldDisplayMeaningOfSignature { get; set; }
        public bool ShouldSignEidasSignatureFlow { get; set; }
    }
}