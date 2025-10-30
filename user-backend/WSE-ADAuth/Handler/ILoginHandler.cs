// Ignore Spelling: Auth WSE

using Common.Models.Users;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WSE_ADAuth.Models;

namespace WSE_ADAuth.Handler
{
    public interface ILoginHandler
    {
        Task<UserTokens> DoDirectExternalLogin(string token);
        Task<string> DoLoginToClientFrontEnd(LoginToClient loginToClient);
        Task<string> DoAuthToSigner(SignerLoginModel signerLoginModel);

        Task<string> DoDirectAuthSignerSamlHostedAppLogin(SignerLoginModel signerLoginModel);
    }
}
