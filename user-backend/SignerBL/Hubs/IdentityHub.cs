using Common.Interfaces.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using SignerBL.Hubs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignerBL.Hubs
{
    public class IdentityHub : Hub
    {
        private IMessageMQSignerIdentityConnector _messageMQSignerIdentityConnector;

        public IdentityHub(IMessageMQSignerIdentityConnector messageMQSignerIdentityConnector)
        {
            _messageMQSignerIdentityConnector = messageMQSignerIdentityConnector;
        }
        public void Connect(string signerToken)
        {
            if (_messageMQSignerIdentityConnector.IsRabbitActive)
            {
                var identityEvent = new BaseIdentityEvent();
                identityEvent.Function = IdentityEvents.CONNECT;
                identityEvent.ConnectionId = Context.ConnectionId;
                identityEvent.RoomToken = signerToken;
                _messageMQSignerIdentityConnector.SendLiveMessage(identityEvent);
            }
            else
            {
                Groups.AddToGroupAsync(Context.ConnectionId, signerToken);
            }

        }

        public void SignerDone(string signerToken, string token)
        {
            if (_messageMQSignerIdentityConnector.IsRabbitActive)
            {
                var identityEvent = new BaseIdentityEvent();
                identityEvent.Function = IdentityEvents.IDENTITY_DONE;
                identityEvent.ConnectionId = Context.ConnectionId;
                identityEvent.RoomToken = signerToken;
                identityEvent.Token = token;
                _messageMQSignerIdentityConnector.SendLiveMessage(identityEvent);
            }
            else
            {
                Clients.OthersInGroup(signerToken).SendAsync(IdentityEvent.ON_IDENTITY_DONE, token);
            }
        }
    }

    public static class IdentityEvents
        {
        public const string IDENTITY_DONE = "IdentityDone";
        public const string CONNECT = "Connect";
    }

}
