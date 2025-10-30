namespace Common.Models
{
    using Common.Enums;
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using System;
    using System.Collections.Generic;

    public class Contact
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneExtension { get; set; }
        public string Phone { get; set; }
        public DateTime LastUsedTime { get; set; }
        public CreationSource CreationSource { get; set; }
        public ContactStatus Status { get; set; }
        public SendingMethod DefaultSendingMethod { get; set; }
        public IEnumerable<Seal> Seals { get; set; }
        
        public string SearchTag { get; set; }

        public Contact()
        {
            Seals = new List<Seal>();
            Status = ContactStatus.Activated;
        }
               
    }
}
