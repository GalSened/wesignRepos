using Common.Handlers.RabbitMQ.Models;
using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Common.Models.Documents.SplitSignature;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Hubs
{
    public class SmartCardPrepareSmartCardParameterForSignatureConsumedProcess : ISmartCardConsumedProcess
    {
        private readonly ISmartCardSigningProcess _smartCardSigningProcess;

        public SmartCardPrepareSmartCardParameterForSignatureConsumedProcess(ISmartCardSigningProcess smartCardSigningProcess)
        {
            _smartCardSigningProcess = smartCardSigningProcess;
        }

        public  Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            var smartCardProcessInput = _smartCardSigningProcess.GetSmartCardProcessInputByToken(baseSmartCardEvent.RoomToken);
            if (smartCardProcessInput != null)
            {
                var newSignatureFieldData = JsonConvert.DeserializeObject<SignatureFieldData>(baseSmartCardEvent.Data.ToString());
                var doc = smartCardProcessInput.Documents.FirstOrDefault();
                if (!doc.SignatureFields.Exists(x => x.Name == newSignatureFieldData.Name))
                {
                    doc.SignatureFields.Add(newSignatureFieldData);
                    _smartCardSigningProcess.UpdateSmartCardInput(baseSmartCardEvent.RoomToken, smartCardProcessInput);
                }
            }
            return Task.CompletedTask;
        }
    }
}
