using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.MessageSending
{
    public interface ISendingMessageHandlerFactory
    {
        IMessageSender Create();

    }
}
