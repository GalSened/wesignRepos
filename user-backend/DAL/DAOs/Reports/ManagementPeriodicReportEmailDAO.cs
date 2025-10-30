using Common.Models.ManagementApp;
using DAL.DAOs.Management;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Reports
{
    [Table("ManagementPeriodicReportEmail")]
    public class ManagementPeriodicReportEmailDAO
    {
        [Key]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid PeriodicReportId { get; set; }
        public virtual ManagementPeriodicReportDAO Report { get; set; }

        public ManagementPeriodicReportEmailDAO() {}
        public ManagementPeriodicReportEmailDAO(ManagementPeriodicReportEmail reportEmail)
        {
            Id = reportEmail.Id;
            Email = reportEmail.Email;
            PeriodicReportId = reportEmail.PeriodicReportId;
        }
    }
}
