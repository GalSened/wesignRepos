using System.Collections.Generic;

namespace WeSignManagement.Models.Companies
{
    public class AllCompaniesResponseDTO
    {
        public IEnumerable<CompanyBaseResponseDTO> Companies { get; set; }
    }
}
