using Common.Enums.Users;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DAOs.Users
{
    [Table("UserOtpDetails")]
    public class UserOtpDetailsDAO
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public UserOtpMode OtpMode { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
