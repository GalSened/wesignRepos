using Common.Enums.Oauth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
   public class IdentityFlowResult
    {
        public string IdentityFlowURL { get; set; }
        public string FirstName { get; set; }
        public string Id { get; set; }
        public string LastName { get; set; }    
        public string PersonalId { get; set; }
        public string DocumentNumber { get;  set; }
        public VisualIdentityProcessResult ProcessResult { get; set; } = VisualIdentityProcessResult.Failed;
        
    }
}
