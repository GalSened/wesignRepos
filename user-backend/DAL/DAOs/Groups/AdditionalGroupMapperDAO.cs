using Common.Enums.Groups;
using Common.Models;
using DAL.DAOs.Companies;
using DAL.DAOs.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DAOs.Groups
{
    [Table("AdditionalGroupsMapper")]
    public class AdditionalGroupMapperDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }

        public virtual CompanyDAO Company { get; set; }
        public virtual UserDAO User { get; set; }
        public virtual GroupDAO Group { get; set; }

        public AdditionalGroupMapperDAO() { }
        public AdditionalGroupMapperDAO(AdditionalGroupMapper additionalGroupMapper)
        {
            Id = additionalGroupMapper.Id == Guid.Empty ? default : additionalGroupMapper.Id;
            CompanyId = additionalGroupMapper.CompanyId == Guid.Empty ? default : additionalGroupMapper.CompanyId;
            UserId = additionalGroupMapper.UserId == Guid.Empty ? default : additionalGroupMapper.UserId;
            GroupId = additionalGroupMapper.GroupId == Guid.Empty ? default : additionalGroupMapper.GroupId;
        }
    }
}
