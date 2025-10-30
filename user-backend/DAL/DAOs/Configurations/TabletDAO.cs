using Common.Models.Configurations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Configurations
{
    [Table("Tablets")]
    public class TabletDAO
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }

        public TabletDAO()
        {

        }
        public TabletDAO(Tablet tablet)
        {
            Id = tablet.Id;
            Name = tablet.Name;
            CompanyId = tablet.CompanyId;
        }
    }
}
