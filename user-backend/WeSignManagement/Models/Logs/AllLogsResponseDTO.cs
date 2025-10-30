using System.Collections.Generic;

namespace WeSignManagement.Models.Logs
{
    public class AllLogsResponseDTO
    {
        public IEnumerable<LogMessageDTO> Logs { get; set; }
    }
}
