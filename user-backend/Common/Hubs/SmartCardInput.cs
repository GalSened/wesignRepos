using Common.Models.Documents.Signers;
using Common.Models.Documents.SplitSignature;
using System;
using System.Collections.Generic;

namespace Common.Hubs
{
    public class SmartCardInput
    {
        public Guid CollectionId { get; set; }    
        public List<string> Clients { get; set; } = new List<string>();
        public List<DocumentSplitSignatureDataProcessInput> Documents { get; set; } = new List<DocumentSplitSignatureDataProcessInput>();  
        public SignerTokenMapping SignerTokenMapping { get;  set; }
        //public bool IsGovDoc { get; set; }


        //public SmartCardInput()
        //{
        //    Clients = new List<string>();
        //    Documents = new List<DocumentSplitSignatureDataProcessInput>();
        //}
    }

}
