namespace Common.Handlers.SendingMessages
{
    using Common.Enums.Documents;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Interfaces.Emails;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Emails;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    public class SmtpProviderHandler : IEmailProvider
    {

        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEncryptor _encryptor;
        private readonly IMemoryCache _memoryCache;

        public SmtpProviderHandler(ILogger logger,  IServiceScopeFactory scopeFactory, IEncryptor encryptor, IMemoryCache memoryCache)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _encryptor = encryptor;
            _memoryCache = memoryCache;
        }


        public Task Send(Email email, SmtpConfiguration configuration)
        {

            if (configuration == null || string.IsNullOrWhiteSpace(configuration.Server) ||
               configuration.Port < 1 || string.IsNullOrWhiteSpace(configuration.From))
            {
                throw new Exception($"Smtp configuration missing (server, port or from address)");
            }
            return Send(configuration, email);
        }

        public Task Send(SmtpConfiguration configuration, Email email)
        {
            try
            {
                SmtpClient client = new SmtpClient(configuration.Server)
                {
                    Port = configuration.Port,
                    EnableSsl = configuration.EnableSsl
                };

                if (!string.IsNullOrWhiteSpace(configuration.Password) && !string.IsNullOrWhiteSpace(configuration.User))
                {
                    client.Credentials = new NetworkCredential(configuration.User, _encryptor.Decrypt(configuration.Password));
                }
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(configuration?.From),
                    Subject = email?.Subject,
                    IsBodyHtml = true,
                    Body = email?.HtmlBody.ToString(),
                };
                mail.To.Add(email?.To);
                foreach (var item in email?.Attachments)
                {
                    var attachment = new Attachment(item.ContentStream, item.Name);
                    mail.Attachments.Add(attachment);
                }
                email.MailMessage = mail;
                client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                client.SendAsync(mail, email);


            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send mail using SMTP client");

            }
            return Task.CompletedTask;
        }
    

        private async void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                var email = (Email)e.UserState;
                string emails = email.To.ToString();
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyServiceDocumentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();

                    if (e.Cancelled || e.Error != null)
                    {
                        _logger.Error("Email to [{Emails}] Failed.{NewLine1}{Error}{NewLine}{ErrorInnerExceptionMessage}", 
                            emails, Environment.NewLine, e.Error, Environment.NewLine, e.Error.InnerException?.Message);
                        if (email?.DocumentCollection != null)
                        {
                            await dependencyServiceDocumentCollectionConnector.UpdateStatus(email.DocumentCollection, email.DocumentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed);
                        }
                    }
                    else
                    {
                        _logger.Information("Successfully send mail to [{Emails}], Link [{Link}]", emails, email?.HtmlBody?.Link);
                        if (email?.DocumentCollection != null)
                        {
                            var sent = _memoryCache.Get<bool>(email.DocumentCollection.Id + "_" + email.DocumentCollection.DocumentStatus.ToString());
                            if (sent == null || !sent)
                            {
                                _memoryCache.Set(email.DocumentCollection.Id + "_" + email.DocumentCollection.DocumentStatus.ToString(), true, TimeSpan.FromSeconds(15));
                                if (email.DocumentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                                    await dependencyServiceDocumentCollectionConnector.UpdateStatus(email.DocumentCollection, email.DocumentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : email.DocumentCollection.DocumentStatus == DocumentStatus.Viewed ? DocumentStatus.Viewed : DocumentStatus.Sent);
                            }

                            if (email.DocumentCollection.DocumentStatus != DocumentStatus.Signed && email.DocumentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                            {
                                var signer = email.DocumentCollection.Signers?.FirstOrDefault(x => x.Contact?.Email == email.To && x.SendingMethod == SendingMethod.Email);
                                if (signer != null)
                                {
                                    await dependencyServiceDocumentCollectionConnector.UpdateSignerSendingTime(email.DocumentCollection, signer);
                                    _logger.Debug("Update Signer UpdateSignerSendingTime");
                                    _logger.Debug("signer {SignerId} sent time {SignerTimeSent}", signer.Id, signer.TimeSent);
                                }
                                else
                                {
                                    _logger.Debug("Signer is null - can't UpdateSignerSendingTime");
                                }
                            }
                        }
                    }
                }
                //if Dispose not happen here the email will be canceled
                email?.MailMessage.Dispose();
                (sender as SmtpClient)?.Dispose();

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed in  smtpProvide CompletedCallback event");
            }
        }

    }
}
