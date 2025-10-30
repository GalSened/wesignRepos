namespace Common.Models.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;

    public class Email 
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public HtmlBody HtmlBody { get; set; }
        //public IList<Attachment> Attachments { get; set; }
        public IList<EmailAttachement> Attachments { get; set; }
        public DocumentCollection DocumentCollection { get; set; }
        public MailMessage MailMessage { get; set; }
        public string DisplayName { get; set; }

        public Email()
        {
            Attachments = new List<EmailAttachement>();
            HtmlBody = new HtmlBody();            
        }
    }
}
