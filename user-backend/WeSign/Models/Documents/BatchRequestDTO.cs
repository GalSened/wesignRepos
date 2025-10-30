using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents
{
    public class BatchRequestDTO
    {
        public string[] Ids { get; set; }
    }

    public class DownloadBatchRequestDTO : BatchRequestDTO { }
    public class DeleteBatchRequestDTO : BatchRequestDTO { }
}
