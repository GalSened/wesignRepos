using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Documents
{
    public class RecordsBatch
    {
        public List<Guid> Ids { get; set; }
        public RecordsBatch()
        {
            Ids = new List<Guid>();
        }
    }
}
