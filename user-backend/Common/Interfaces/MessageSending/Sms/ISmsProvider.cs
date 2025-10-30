namespace Common.Interfaces.MessageSending.Sms
{
    using Common.Models;
    using Common.Models.Configurations;
    using Models.Sms;
    using System;
    using System.Collections.Generic;

    public interface ISmsProvider
    {
        void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null);
        void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration);
    }
}
