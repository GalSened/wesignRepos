using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Configurations
{
    [Table("Notifications")]
    public class NotificationDAO
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public NotificationMessageType MessageType { get; set; }
        public bool ShouldSend { get; set; }
        public int FrequencyInDays { get; set; }
        public DateTime Time { get; set; }
        public string Text { get; set; }
    }
}
