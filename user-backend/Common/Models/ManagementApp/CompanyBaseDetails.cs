using System;
using System.Collections.Generic;

namespace Common.Models.ManagementApp
{
    public class CompanyBaseDetails
    {       
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<(Guid, string)> UsersEmails { get; set; }
        public string ProgramName { get; set; }
        public DateTime? ExipredTime { get; set; }
        public long Documents { get; set; }
        public long Templates { get; set; }
        public long Users { get; set; }
        public long Sms { get; set; }
        public long VisualIdentifications { get; set; }
        public CompanyBaseDetails(Company company)
        {
            if (company != null)
            {
                Id = company.Id;
                Name = company.Name;
                ExipredTime = company.ProgramUtilization?.Expired;
                Documents = company.ProgramUtilization?.DocumentsUsage ?? 0;
                Templates = company.ProgramUtilization?.Templates ?? 0;
                Users = company.ProgramUtilization?.Users ?? 0;
                Sms = company.ProgramUtilization?.SMS ?? 0;
                VisualIdentifications = company.ProgramUtilization?.VisualIdentifications ?? 0;
            }
        }
    }
}
