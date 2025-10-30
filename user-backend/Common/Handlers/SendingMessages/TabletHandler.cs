using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.Configurations;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Common.Models.Settings;
using Serilog;
using System.Collections.Generic;

namespace Common.Handlers.SendingMessages
{
    public class TabletHandler : IMessageSender
    {

        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;

        public TabletHandler(GeneralSettings generalSettings, ILogger logger)
        {
            _generalSettings = generalSettings;
            _logger = logger;
        }


        public async Task Send(Configuration appConfiguration, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            
           await Task.Run(async ()=>
            {
                var connection = new HubConnectionBuilder().WithUrl(_generalSettings.AgentHubEndpoint).WithAutomaticReconnect()
                .Build();
                try
                {
                    _logger.Debug("TabletHandler - Send - Start connection to agent hub endpoint at [{AgentHubEndpoint}]", _generalSettings.AgentHubEndpoint);
                    await connection.StartAsync();
                    string connectionId = $"{companyConfiguration.CompanyId}_{messageInfo.Contact.Name}";
                    await connection.InvokeAsync("Connect", connectionId);
                    _logger.Debug("TabletHandler - Send - Send link [{Link}] to tablet [{ContactName}]", messageInfo.Link, messageInfo.Contact.Name);
                    await connection.InvokeAsync("SendLink", connectionId, messageInfo.Link);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "TabletHandler - Error in Send function");
                }
            });
            
        }

        public async Task SendBatch(Configuration appConfiguration, CompanyConfiguration companyConfiguration, List<MessageInfo> messageInfo)
        {
            // for now send one by one

            foreach (var message in messageInfo)
            {
               await Send(appConfiguration, companyConfiguration, message);
            }
            
        }
    }
}
