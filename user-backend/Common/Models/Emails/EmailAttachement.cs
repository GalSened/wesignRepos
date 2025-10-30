using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Models.Emails
{
    public class EmailAttachement
    {
        public Guid ContentId { get; set; }
        public string Name { get; set; }
        public Stream ContentStream { get; set; }

        public EmailAttachement(string name ,Stream stream)
        {
            Name = name;
            ContentStream = stream;
            ContentId = Guid.NewGuid();
        }

        public EmailAttachement(string name, Stream stream, Guid id)
        {
            Name = name;
            ContentStream = stream;
            ContentId = id;
        }

    }
}
