using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents
{
    public class ShareDTO 
    {
        public Guid DocumentCollectionId { get; set; }
        public string SignerName { get; set; }

        public string SignerMeans { get; set; }
    }

}
