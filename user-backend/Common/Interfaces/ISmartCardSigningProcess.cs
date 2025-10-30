using Common.Hubs;
using Common.Models.Documents.SplitSignature;
using CTInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISmartCardSigningProcess
    {
        Task<string> CloseSigningProcess(string roomToken);
        void CreateGroup(string roomToken, SmartCardInput smartCardInput);
        bool EmbadeSignatureInDoc(string roomToken, byte[] signedHash, string fieldName);
        SmartCardInput GetSmartCardProcessInputByToken(string roomToken);
        List<SignatureFieldData> PrepareSignatureNextField(string roomToken);
        void UpdateSmartCardInput(string roomId, SmartCardInput smartCardProcessInput);
        PrepResponse PrepareSignatureForField(DocumentSplitSignatureDataProcessInput smartCardProcessInput, List<SignatureFieldData> signatureField);
        void SetItemRemoveEvent( Action<string,SmartCardInput> itemRemoveEvent);
        void UpdateProcessInputByToken(string oldRoomToken,string newRoomToken);
    }
}
