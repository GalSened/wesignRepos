using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using PdfHandler.Enums;
using SignerBL.Hubs.Models;

using System;

namespace SignerBL.Hubs
{
    public class LiveHub : Hub 
    {

      

        private IMessageMQLiveConnector _rabbitMQLiveConnector;

        public LiveHub(IMessageMQLiveConnector rabbitMQLiveConnector) {
            _rabbitMQLiveConnector = rabbitMQLiveConnector;

        }

        public void Connect(string documentCollectionToken)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_CONNECT;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Groups.AddToGroupAsync(Context.ConnectionId, documentCollectionToken);
            }
        }

        public void Scroll(string documentCollectionToken, ScrollParams scrollParams)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_SCROLL;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                data.Data = scrollParams;
                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_SCROLL, scrollParams.Left, scrollParams.Top, scrollParams.Page);
            }
        }

        public void SignerDecline(string documentCollectionToken)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_SIGNER_DECLINE;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_SIGNER_DECLINE);
            }
        }

        public void PingToOthers(string documentCollectionToken, bool isSenderView)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_PING_TO_OTHERS;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                //data.Data =  new Boolean();   
                data.Data = isSenderView;
                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_PING_TO_OTHERS, isSenderView);
            }
        }

        public void SetFieldData(string documentCollectionToken, string documentId, FieldRequest fieldRequest)
        {

            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent fieldRequestEvent = new BaseLiveEvent();
                fieldRequestEvent.Function = LiveEvents.ON_FIELD_DATA_CHANGED;
                fieldRequestEvent.DocumentCollectionToken = documentCollectionToken;
                fieldRequestEvent.ConnectionId = Context.ConnectionId;
                fieldRequestEvent.Data = fieldRequest;
                fieldRequestEvent.DocumentId = documentId;
                _rabbitMQLiveConnector.SendLiveMessage(fieldRequestEvent);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_FIELD_DATA_CHANGED, documentId, fieldRequest);
            }
        }

        public void Zoom(string documentCollectionToken, double zoomLevel)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_ZOOM;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                data.Data = zoomLevel;
                
                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_ZOOM, zoomLevel);
            }
        }

        public void ChangeBackgroud(string documentCollectionToken, bool bright)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_CHANGE_BACKGROUD;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                data.Data = bright;

                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_CHANGE_BACKGROUD, bright);
            }
        }

        public void NotifySigningResult(string documentCollectionToken, bool isSigningSuccess)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_FINISH_SIGNING;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;
                data.Data = isSigningSuccess;

                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_FINISH_SIGNING, isSigningSuccess);
            }
        }

        public void FinishAsSender(string documentCollectionToken)
        {
            if (_rabbitMQLiveConnector.IsRabbitActive)
            {
                //----
                BaseLiveEvent data = new BaseLiveEvent();
                data.Function = LiveEvents.ON_FINISH_AS_SENDER;
                data.DocumentCollectionToken = documentCollectionToken;
                data.ConnectionId = Context.ConnectionId;           
                _rabbitMQLiveConnector.SendLiveMessage(data);
            }
            else
            {
                Clients.OthersInGroup(documentCollectionToken).SendAsync(LiveEvents.ON_FINISH_AS_SENDER);
            }
        }

    }
}
