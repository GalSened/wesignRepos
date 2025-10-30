using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.MessageSending.Mail
{
    public interface IEmailTypeHandler
    {
        IEmailType ExecuteCreation(MessageType messageType);
    }
}
