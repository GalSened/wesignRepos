using Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Oauth
{
    public interface IVisualIdentity
    {
         Task<IdentityCreateFlowResult> StartVisualIdentityFlow(IdentityFlow identityFlow);

        Task<IdentityFlowResult> ReadVisualIdentityReqults(IdentityFlow identityFlow);


    }
}
