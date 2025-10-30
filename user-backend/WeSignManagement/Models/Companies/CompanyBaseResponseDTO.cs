using Common.Models.ManagementApp;
using System;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies
{
    public class CompanyBaseResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<(Guid, string)> CompanyAdminUsers { get; set; }
        public string ProgramName { get; set; }
        public DateTime? ExipredTime { get; set; }
        public long Documents { get; set; }
        public long Templates { get; set; }
        public long Users { get; set; }
        public long Sms { get; set; }
        public long VisualIdentifications { get; set; }

        public CompanyBaseResponseDTO(CompanyBaseDetails companyDetail)
        {
            if (companyDetail != null)
            {
                Id = companyDetail.Id;
                Name = companyDetail.Name;
                ProgramName = companyDetail.ProgramName;
                CompanyAdminUsers = companyDetail.UsersEmails;
                ExipredTime = companyDetail.ExipredTime;
                Users = companyDetail.Users;
                Documents = companyDetail.Documents;
                Templates = companyDetail.Templates;
                Sms = companyDetail.Sms;
                VisualIdentifications = companyDetail.VisualIdentifications;
                

            }
        }
    }
}
