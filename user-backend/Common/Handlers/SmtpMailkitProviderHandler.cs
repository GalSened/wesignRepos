using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Interfaces.Emails;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Serilog;
using Common.Models.Configurations;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using System.Linq;
using Common.Enums.Documents;
using MailKit;
using Email = Common.Models.Emails.Email;
using Common.Models;
using Common.Models.Documents.Signers;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Common.Models.Settings;
using System.Collections.Concurrent;
using System.Timers;

namespace Common.Handlers
{
    public class SmtpMailkitProviderHandler : IEmailProvider
    {
        private readonly ILogger _logger;

        
        private readonly IEncryptor _encryptor;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<GeneralSettings> _generalSettings;
        private readonly static ConcurrentQueue<Tuple<SmtpConfiguration, MimeMessage>> _mailsToSend = new ConcurrentQueue<Tuple<SmtpConfiguration, MimeMessage>>();
        private readonly static Timer _timer = new Timer() { Interval = 8000};
        private static bool _setTimer = false;
        private static bool _timerInProcess = false;
        private static IServiceScopeFactory _persistentScopeFactory;



        private readonly string SIGNER_URL;
        private readonly string SIGNATURE = "signature";

        public SmtpMailkitProviderHandler(ILogger logger,  IServiceScopeFactory scopeFactory, IEncryptor encryptor,
            IMemoryCache memoryCache, IOptions<GeneralSettings> generalSettings)
        {
            _logger = logger;

            _encryptor = encryptor;
            _memoryCache = memoryCache;
            _generalSettings = generalSettings;

            SIGNER_URL = _generalSettings.Value.SignerFronendApplicationRoute;
            if (!_setTimer)
            {
                _timer.Elapsed += TimerElapsed;
                _timer.AutoReset = true;
                _timer.Start();
                _setTimer = true;
                _persistentScopeFactory = scopeFactory;
            }

        }   

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_timerInProcess)
                {
                    return;
                }
                Dictionary<string, SmtpClient> smtpClients = new Dictionary<string, SmtpClient>();
                Dictionary<string, int> smtpClientsSent = new Dictionary<string, int>();
                try
                {
                    _timerInProcess = true;
                    _timer.Stop();


                    while (_mailsToSend.TryDequeue(out Tuple<SmtpConfiguration, MimeMessage> email))
                    {
                        try
                        {
                            _logger.Debug("Email , from: {EmailFrom} - to: {EmailTo} dequeue and start processing ", email.Item2.From, email.Item2.To);

                            if (!smtpClients.ContainsKey(email.Item1.Server))
                            {
                                smtpClients.Add(email.Item1.Server, await GetSmtp(email.Item1));
                                smtpClientsSent.Add(email.Item1.Server, 0);

                            }

                            if(smtpClientsSent[email.Item1.Server] > 10)
                            {
                                try
                                {
                                    await Task.Delay(70);
                                    await smtpClients[email.Item1.Server].DisconnectAsync(true);
                                    await Task.Delay(70);
                                    smtpClients[email.Item1.Server].Dispose();
                                    smtpClients.Remove(email.Item1.Server);
                                    smtpClients.Add(email.Item1.Server, await GetSmtp(email.Item1));
                                    smtpClientsSent[email.Item1.Server] = 0;
                                    await Task.Delay(1000);
                                }
                                catch
                                {
                                    // do nothing
                                }


                            }
                            await smtpClients[email.Item1.Server].SendAsync(email.Item2);
                            smtpClientsSent[email.Item1.Server]++;
                            await Task.Delay(70);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Failed while trying to create or sent mail {EmailServer}, to {EmailTo}", email.Item1.Server, email.Item2.To);
                        }

                    }


                }
                finally
                {
                    foreach (var smtpClient in smtpClients.Values)
                    {
                        try
                        {
                            await smtpClient.DisconnectAsync(true);
                            await Task.Delay(70);
                            smtpClient.Dispose();
                        }
                        catch
                        {
                            // do nothing
                        }
                    }

                    _timer.Start();
                    _timerInProcess = false;

                }
                
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed mailKit timerElapsed timer Event");
            }
        }

        private async Task<SmtpClient> GetSmtp(SmtpConfiguration configuration)
        {

            SmtpClient smtp = new SmtpClient();

            if (configuration.EnableSsl)
            {
                await smtp.ConnectAsync(configuration.Server, configuration.Port, SecureSocketOptions.StartTls);
               
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(configuration.Password) && !string.IsNullOrWhiteSpace(configuration.User))
                {
                    await smtp.ConnectAsync(configuration.Server, configuration.Port);
                    var password = _encryptor.Decrypt(configuration.Password);
                    await smtp.AuthenticateAsync(configuration.User, password);
                   
                }
                else
                {                    
                    await smtp.ConnectAsync(configuration.Server, configuration.Port, SecureSocketOptions.None) ;
                }
            }

            smtp.MessageSent += SendCompletedCallback;

            return smtp;


        }

        public  Task Send(Email email, SmtpConfiguration configuration)
        {

            if (configuration == null || string.IsNullOrWhiteSpace(configuration.Server) ||
               configuration.Port < 1 || string.IsNullOrWhiteSpace(configuration.From))
            {
                throw new Exception($"Smtp configuration missing (server, port or from address)");
            }
            return Send(configuration, email);
        }



        public  Task Send(SmtpConfiguration configuration, Email email)
        {
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(configuration?.From, $"{email.DisplayName} via WeSign"),
                Subject = email?.Subject,
                IsBodyHtml = true,
                Body = email?.HtmlBody.ToString(),
            };
            
            var mail = (MimeMessage)mailMessage;

            if ((email.DocumentCollection != null) && (email.DocumentCollection.Signers != null))
            {
                var signer = email.DocumentCollection.Signers.ToList().Find(x => x.Contact.Email == email.To);
                if (signer != null)
                    mail.MessageId = email.DocumentCollection != null ? signer.Id.ToString() : String.Empty;

            }

            mail.Sender = new MailboxAddress("", configuration.From);
            mail.To.Add(MailboxAddress.Parse(email.To));

            var multipart = new Multipart("mixed")
            {
                mail.Body
            };
            foreach (var attachment in email.Attachments)
            {
                multipart.Add(new MimePart()
                {
                    FileName = attachment.Name,
                    IsAttachment = true,

                    Content = new MimeContent(attachment.ContentStream),
                    ContentId = attachment.ContentId.ToString(),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
                });
            }
            mail.Body = multipart;
            var item = new Tuple<SmtpConfiguration, MimeMessage>(configuration,mail );
            _mailsToSend.Enqueue(item);
            return Task.CompletedTask;
        }

        private async Task<(string, bool getValidLink)> GetLinkFromSignerId(string signerId)
        {
            
            if (string.IsNullOrWhiteSpace(signerId))
            {
                
                return (string.Empty, false);
            }
            else
            {
                using (var scope = _persistentScopeFactory.CreateScope())
                {
                    ISignerTokenMappingConnector dependencyServiceSignerTokenMappingConnector = scope.ServiceProvider.GetService<ISignerTokenMappingConnector>();
                    SignerTokenMapping signerTokenMapping = await dependencyServiceSignerTokenMappingConnector.Read(new SignerTokenMapping() { SignerId = Guid.Parse(signerId) });

                    if (string.IsNullOrEmpty(signerTokenMapping?.GuidToken.ToString()))
                    {

                        return (string.Empty, false);
                    }

                    string link = String.Join("/", SIGNER_URL, SIGNATURE, signerTokenMapping.GuidToken.ToString());

                    return (link, true);
                }
            }
        }

        private async void SendCompletedCallback(object sender, MessageSentEventArgs e)
        {
            try
            {
                var email = e.Message;
                string emails = email.To.ToString();
                
                var signerId = email.MessageId;
                string link = string.Empty;
                bool valid = false;
                using (var scope = _persistentScopeFactory.CreateScope())
                {
                    var dependencyServiceDocumentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();

                    DocumentCollection documentCollection = null;
                    if (!string.IsNullOrEmpty(signerId))
                        documentCollection = await dependencyServiceDocumentCollectionConnector.ReadBySignerId(new Signer() { Id = Guid.Parse(signerId) });

                    if (documentCollection != null)
                    {

                        _logger.Debug("DocCollectionId {DocCollection} in after email sent event to signer {Signer} docStatus {DosStatus}", documentCollection.Id, signerId, documentCollection.DocumentStatus);
                        var sent = _memoryCache.Get<bool>(documentCollection.Id + "_" + documentCollection.DocumentStatus.ToString()) ;
                        if (sent == null || !sent)
                        {
                            _memoryCache.Set(documentCollection.Id + "_" + documentCollection.DocumentStatus.ToString(), true, TimeSpan.FromSeconds(15));
                            if ((documentCollection.DocumentStatus == DocumentStatus.Created ||
                                documentCollection.DocumentStatus == DocumentStatus.Draft) && documentCollection.SignedTime == DateTime.MinValue )
                            {
                                _logger.Debug("DocCollectionId {DocCollection} in after email sent event to signer {Signer} docStatus {DosStatus} update status to {NewStatus}", documentCollection.Id, signerId, documentCollection.DocumentStatus, DocumentStatus.Sent);
                                await dependencyServiceDocumentCollectionConnector.UpdateStatus(documentCollection, DocumentStatus.Sent);
                            }


                        }
                        if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                        {
                            Signer signer = documentCollection.Signers?.FirstOrDefault(x => x.SendingMethod == SendingMethod.Email && x.Id.ToString() == signerId);
                            if (signer != null && signer.Status != Enums.Contacts.SignerStatus.Signed)
                            {

                                await dependencyServiceDocumentCollectionConnector.UpdateSignerSendingTime(documentCollection, signer);
                                _logger.Debug("UpdateSignerSendingTime signer {SignerId} sent time {SignerTimeSent} signer status is {signerStatus}", signer.Id, signer.TimeSent,signer.Status);

                            }
                            else
                            {
                                _logger.Debug("Signer is null - can't UpdateSignerSendingTime");
                            }

                        }
                    }
                    // for now not requst
                    (link, valid) = await GetLinkFromSignerId( signerId);
                }
                if (valid)
                    _logger.Information("Successfully send mail to [{Emails}], Link [{Link}]", emails, link);


                if (e.Message is IDisposable)
                {
                    try
                    {
                        e.Message.Dispose();
                    }
                    catch
                    {
                        _logger.Warning("MimeMessage in SmtpMailkitProviderHandler failed to Dispose");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed in mailKit CompletedCallback Event ");
            }
        }
            
    }
}
