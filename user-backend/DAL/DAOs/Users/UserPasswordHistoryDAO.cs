using Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Users
{
    [Table("UsersPasswordHistory")]
    public class UserPasswordHistoryDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
        public DateTime CreationTime { get; set; }
        public UserPasswordHistoryDAO() { }
        public UserPasswordHistoryDAO(UserPasswordHistory userPasswordHistory)
        {
            Id = userPasswordHistory.Id;
            UserId = userPasswordHistory.UserId;
            Password = userPasswordHistory.Password;
            CreationTime = userPasswordHistory.CreationTime;
        }
    }
}
