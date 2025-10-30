using Common.Handlers.RabbitMQ.Models;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Common.Models.Documents.SplitSignature;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.SmatCard
{
    public class SmartCardSendHashConsumedProcess : ISmartCardConsumedProcess
    {
        private ISmartCardSigningProcess _smartCardSigningProcess;
        private IHubContext<SmartCardSigningHub> _hubContext;

        public SmartCardSendHashConsumedProcess(ISmartCardSigningProcess smartCardSigningProcess, IHubContext<SmartCardSigningHub> hubContext)
        {
            _smartCardSigningProcess = smartCardSigningProcess;
            _hubContext = hubContext;
        }

        public async Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            var smartCardProcessInput = _smartCardSigningProcess.GetSmartCardProcessInputByToken(baseSmartCardEvent.RoomToken);
            var inputSignatureData = JsonConvert.DeserializeObject<SignatureFieldData>(baseSmartCardEvent.Data.ToString());

            if (smartCardProcessInput != null)
            {

               var document =  smartCardProcessInput.Documents.First();
                var signatureField = document.SignatureFields.FirstOrDefault(x => x.Name == inputSignatureData.Name);
                if (signatureField != null && signatureField.Hash != inputSignatureData.Hash)
                {
                    var signatureFields = new List<SignatureFieldData>();
                    signatureFields.Add(signatureField);
                    PrepResponse sigFieldPrepareRespnse = _smartCardSigningProcess.PrepareSignatureForField(document, signatureFields);
                    signatureField.Hash = sigFieldPrepareRespnse.Hash;
                    signatureField.PrepareSignaturePdfResult = sigFieldPrepareRespnse.PDF;
                    _smartCardSigningProcess.UpdateSmartCardInput(baseSmartCardEvent.RoomToken, smartCardProcessInput);


                    await _hubContext.Clients.Group(baseSmartCardEvent.RoomToken).SendAsync(SmartCardEventsConstants.GET_HASH, signatureField.Hash, signatureField.Name, baseSmartCardEvent.RoomToken);
                }
            }

        }
    }
}
