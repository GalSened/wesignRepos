using Common.Enums.Reports;
using Common.Models.ManagementApp.Reports;
using DAL.DAOs.Reports;
using DAL.DAOs.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Management
{
    [Table("ManagementPeriodicReport")]
    public class ManagementPeriodicReportDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ManagementReportType ReportType { get; set; }
        public DateTime LastTimeSent { get; set; }
        public ManagementReportFrequency ReportFrequency { get; set; }
        public string ReportParameters { get; set; }
        public virtual ICollection<ManagementPeriodicReportEmailDAO> Emails { get; set; }
        public virtual UserDAO User { get; set; }

        public ManagementPeriodicReportDAO(){}
        public ManagementPeriodicReportDAO(ManagementPeriodicReport managementReport)
        {
            Id = managementReport.Id;
            UserId = managementReport.UserId;
            ReportType = managementReport.ReportType;
            LastTimeSent = managementReport.LastTimeSent;
            ReportFrequency = managementReport.ReportFrequency;
            ReportParameters = managementReport.ReportParameters;
        }
    }
}
