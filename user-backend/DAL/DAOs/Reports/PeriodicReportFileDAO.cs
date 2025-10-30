using Common.Models.Reports;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Reports
{
    [Table("PeriodicReportFile")]
    public class PeriodicReportFileDAO
    {
        [Key]
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime CreationTIme { get; set; }

        public PeriodicReportFileDAO(){}
        public PeriodicReportFileDAO(PeriodicReportFile periodicReportFile)
        {
            Id = periodicReportFile.Id;
            Token = periodicReportFile.Token;
            CreationTIme = periodicReportFile.CreationTime;
        }
    }
}
