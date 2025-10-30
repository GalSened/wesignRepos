using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class IdentityFlow
    {
        public Guid SignerToken { get; set; }
        public string Code { get; set; }
        public string FieldName { get; set; }
        public Guid DocumentId { get; set; }
    }
}
