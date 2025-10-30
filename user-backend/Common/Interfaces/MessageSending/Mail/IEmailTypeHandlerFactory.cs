using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.MessageSending.Mail
{
    public interface IEmailTypeHandlerFactory
    {
        IEmailType Create();
    }
}
