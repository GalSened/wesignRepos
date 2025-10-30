using Common.Models;
using System;

namespace WeSignManagement.Models.Companies.Responses
{
    public class GroupDTO
    {
        public Guid Id;
        public string name;
        public Guid companyId;

        public GroupDTO(Group group)
        {
            this.Id = group.Id;
            this.name = group.Name;
            this.companyId = group.CompanyId;
        }
    }
}
