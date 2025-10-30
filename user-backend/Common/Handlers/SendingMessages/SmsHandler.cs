using Comda.Authentication.Models;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Sms;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages
{
    public class SmsHandler : IMessageSender
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;


        public SmsHandler(ILogger logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        public async Task Send(Configuration appConfiguration, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();  
            IProgramConnector programConnector =scope.ServiceProvider.GetService<IProgramConnector>();
            var phone = (messageInfo?.Contact?.Phone ?? "").StartsWith('+') ? messageInfo?.Contact?.Phone ?? "" : $"{messageInfo?.Contact?.PhoneExtension}{messageInfo?.Contact?.Phone}";
            var smsInfo = new Sms()
            {
                Phones = new List<string> { phone },
                Message = messageInfo?.MessageContent
            };
            var smsConfiguration = await _configuration.GetSmsConfiguration(messageInfo?.User, appConfiguration, companyConfiguration);
            var smsProvider = await _configuration.GetSmsProviderHandler(messageInfo?.User, appConfiguration, companyConfiguration);
            if (!await programConnector.CanAddSms(messageInfo?.User) &&  messageInfo?.MessageType == Enums.MessageType.BeforeSigning)
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            smsProvider.SendAsync(smsInfo, smsConfiguration, messageInfo?.DocumentCollection);
            _logger.Debug("Successfully sent SMS from {UserId} : {UserName} to {PhoneNumber}", messageInfo?.User.Id, messageInfo?.User.Name, messageInfo?.Contact.Phone);

            if (messageInfo.MessageType == Enums.MessageType.BeforeSigning)
            {
                IProgramUtilizationConnector programUtilizationConnector = scope.ServiceProvider.GetService<IProgramUtilizationConnector>();
                await programUtilizationConnector.AddSms(messageInfo.User);
                _logger.Debug("Sms utilization used for user {UserName} is: {SmsAmount}", messageInfo.User.Name, 1);


            }
        }

        public async Task SendBatch(Configuration appConfiguration, CompanyConfiguration companyConfiguration, List<MessageInfo> messageInfo)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            List<Tuple<Sms, DocumentCollection>> smsInfo = new List<Tuple<Sms, DocumentCollection>>();
            var user = messageInfo[0].User;

            foreach (var message in messageInfo)
            {
                var phone = (message?.Contact?.Phone ?? "").StartsWith('+') ? message?.Contact?.Phone ?? "" : $"{message?.Contact?.PhoneExtension}{message?.Contact?.Phone}";
                var sms = new Sms()
                {
                    Phones = new List<string> { phone },
                    Message = message?.MessageContent
                };

                smsInfo.Add(new Tuple<Sms, DocumentCollection>(sms, message.DocumentCollection));
            }


            int smsAmount = smsInfo.Count;
            var smsConfiguration = await _configuration.GetSmsConfiguration(user, appConfiguration, companyConfiguration);
            var smsProvider = await _configuration.GetSmsProviderHandler(user, appConfiguration, companyConfiguration);
            if (!await programConnector.CanAddSms(user, smsAmount) && messageInfo[0]?.MessageType == Enums.MessageType.BeforeSigning)
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            smsProvider.SendBatchAsync(smsInfo, smsConfiguration);
            _logger.Debug("Successfully sent SMS from {UserId} : {UserName} to {PhoneAmounts} numbers", user.Id, user.Name, smsInfo.Count);

            if (messageInfo[0]?.MessageType == Enums.MessageType.BeforeSigning)
            {
                IProgramUtilizationConnector programUtilizationConnector = scope.ServiceProvider.GetService<IProgramUtilizationConnector>();
                await programUtilizationConnector.AddSms(user, smsAmount);
                _logger.Debug("Sms utilization used for user {UserName} is: {SmsAmount}", user.Name, smsAmount);
            }

        }
    }
}
