using Common.Handlers.RabbitMQ.Models;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Common.Models.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.SmatCard
{
    public class SmartCardConsumedProcessFactory : ISmartCardConsumedProcessFactory
    {
        private RabbitMQSettings _settings;
        private ILogger _logger;
        private IHubContext<SmartCardSigningHub> _hubContext;
        private IRabbitConnector _rabbitConnector;
        private ISmartCardSigningProcess _smartCardSigningProcess;

        public SmartCardConsumedProcessFactory(IOptions<RabbitMQSettings> settings, ILogger logger,
           IHubContext<SmartCardSigningHub> hubContext, IRabbitConnector rabbitConnector,
            ISmartCardSigningProcess smartCardSigningProcess) {
            _settings = settings.Value;
            _logger = logger;
            _hubContext = hubContext;
            _rabbitConnector = rabbitConnector;
            _smartCardSigningProcess = smartCardSigningProcess;

        }
        public ISmartCardConsumedProcess CreateProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            switch (baseSmartCardEvent.Function)
            {
                case SmartCardEventsConstants.CREATE_GROUP:
                    {

                        return new SmartCardCreateGroupConsumedProcess(_smartCardSigningProcess, _hubContext);
                        
                    }
                case SmartCardEventsConstants.CONNECT:
                    {
                        return new SmartCardConnectConsumedProcess(_smartCardSigningProcess, _hubContext);
                        
                    }
                case SmartCardEventsConstants.LEAVE_GROUP:
                    {
                        return new SmartCardLeaveGroupConsumedProcess(_smartCardSigningProcess, _hubContext);
                    }
                case SmartCardEventsConstants.PREPARE_SMART_CARD_PARAMETER_FOR_SIGNATURE:
                    {
                        return new SmartCardPrepareSmartCardParameterForSignatureConsumedProcess(_smartCardSigningProcess);
                        
                    }
                case SmartCardEventsConstants.SEND_HASH:
                    {
                        return new SmartCardSendHashConsumedProcess(_smartCardSigningProcess, _hubContext);
                    }
                case SmartCardEventsConstants.PROCESS_DONE:
                    {
                        return new SmartCardProcessDoneConsumedProcess(_hubContext);
                        
                    }
                case SmartCardEventsConstants.SEND_MESSAGE:
                    {
                        return new SmartCardSendMessageConsumedProcess(_hubContext);
                        
                    }
                default:
                    {
                        return new SmartCardEmptyConsumedProcess();
                    }
            }
        }
    }
}
