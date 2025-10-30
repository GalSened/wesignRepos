using Common.Enums.Reports;
using Common.Models.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Users
{
    [Table("UserPeriodicReport")]
    public class UserPeriodicReportDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ReportType ReportType { get; set; }
        public DateTime LastTimeSent { get; set; }
        public ReportFrequency ReportFrequency { get; set; }
        public virtual UserDAO User { get; set; }

        public UserPeriodicReportDAO() { }
        public UserPeriodicReportDAO(UserPeriodicReport userReport)
        {
            Id = userReport.Id;
            UserId = userReport.UserId;
            ReportType = userReport.ReportType;
            LastTimeSent = userReport.LastTimeSent;
            ReportFrequency = userReport.ReportFrequency;
        }
    }
}
