using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Programs
{
    public class AllProgramUtilizationHistoriesResponseDTO
    {
        public IEnumerable<Common.Models.Programs.ProgramUtilizationHistory> Reports { get; set; }
    }
}
