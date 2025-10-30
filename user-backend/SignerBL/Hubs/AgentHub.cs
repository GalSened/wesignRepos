using Common.Interfaces.DB;
using Common.Interfaces.RabbitMQ;
using Common.Models.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using SignerBL.Hubs.Models;
using System;
using System.Threading.Tasks;

namespace SignerBL.Hubs
{
    public class AgentHub : Hub
    {
        private readonly IConfigurationConnector _configurationConnector;
        private readonly ILogger _logger;
        private IMessageMQAgentConnector _rabbitMQAgentConnector;
        private ICompanyConnector _companyConnector;

        public AgentHub(IConfigurationConnector configurationConnector, ILogger logger,
            IMessageMQAgentConnector rabbitMQAgentConnector, ICompanyConnector companyConnector)
        {
            _configurationConnector = configurationConnector;
            _logger = logger;
            _rabbitMQAgentConnector = rabbitMQAgentConnector;
            _companyConnector = companyConnector;
        }


        public void Ping(string groupName)
        {
            groupName = groupName.ToLower();
            _logger.Debug("ping from group {GroupName} ", groupName);
            if (_rabbitMQAgentConnector.IsRabbitActive)
            {
                var data = new BaseAgentEvent();
                data.Function = AgentEvents.PING;
                data.RoomToken = groupName;
                data.ConnectionId = Context.ConnectionId;
                _rabbitMQAgentConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.Group(groupName).SendAsync(AgentEvents.PING);
            }


        }
        public async Task Connect(string groupName)
        {

            if (string.IsNullOrWhiteSpace(groupName))
            {
                _logger.Warning("Agent connect empty Group name");
                return;
            }
            groupName = groupName.ToLower();
            await RegisterNewConnection(groupName);

            if (_rabbitMQAgentConnector.IsRabbitActive)
            {
                BaseAgentEvent data = new()
                {
                    Function = AgentEvents.CONNECT,
                    RoomToken = groupName,
                    ConnectionId = Context.ConnectionId
                };
                _rabbitMQAgentConnector.SendLiveMessage(data);
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
        }      

        public void SendLink(string groupName, string link)
        {
            groupName = groupName.ToLower();
            _logger.Debug("A link sent in group {GroupName} ", groupName);
            if (_rabbitMQAgentConnector.IsRabbitActive)
            {
                var data = new BaseAgentEvent();
                data.Function = AgentEvents.SEND_LINK;
                data.RoomToken = groupName;
                data.Link = link;
                data.ConnectionId = Context.ConnectionId;
                _rabbitMQAgentConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(groupName).SendAsync(AgentEvents.ON_LINK_CHANGE, link);
            }
        }

        public void MoveToAd(string groupName)
        {
            groupName = groupName.ToLower();
            _logger.Debug("group {GroupName} move back to ad ", groupName);
            if (_rabbitMQAgentConnector.IsRabbitActive)
            {
                var data = new BaseAgentEvent();
                data.Function = AgentEvents.MOVE_TO_AD;
                data.RoomToken = groupName;
                data.ConnectionId = Context.ConnectionId;
                _rabbitMQAgentConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(groupName).SendAsync(AgentEvents.ON_MOVE_TO_AD);
            }
        }

        private async Task RegisterNewConnection(string groupName)
        {
            try
            {
                
                _logger.Debug("A new device connected to group {GroupName} ", groupName);
                var splitedgroupName = groupName.Split('_', 2, StringSplitOptions.RemoveEmptyEntries) ;
                Guid companyId = Guid.Parse(splitedgroupName[0]);
                Tablet tablet = new Tablet
                {
                    Name = splitedgroupName[1],
                    CompanyId = companyId
                };
                var companyConfiguration = await _companyConnector.ReadConfiguration(new Common.Models.Company { Id = companyId });
                if (companyConfiguration != null && companyConfiguration.EnableTabletsSupport && !await _configurationConnector.IsTabletExist(tablet))
                {
                    await _configurationConnector.CreateTablet(tablet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in Connect to AgentHub");
            }
        }
    }
    public static class AgentEvents
    {
        public const string MOVE_TO_AD = "MoveToAd";
        public const string SEND_LINK = "SendLink";
        public const string CONNECT = "Connect";
        public const string PING = "Ping";
        public const string ON_LINK_CHANGE = "onLinkChange";
        public const string ON_MOVE_TO_AD = "onMoveToAd";
    }
}
