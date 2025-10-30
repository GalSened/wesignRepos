using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Logs
{
    [Table("Logs")]
    public class LogDAO
    {
        [Key]
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public string Exception { get; set; }
    }
}
