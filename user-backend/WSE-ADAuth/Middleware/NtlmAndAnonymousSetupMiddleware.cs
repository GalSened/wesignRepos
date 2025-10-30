using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WSE_ADAuth.Models;
using Serilog;
namespace WSE_ADAuth.Middleware
{
    public class NtlmAndAnonymousSetupMiddleware(RequestDelegate _next, 
        ILogger _logger, IOptions<ADGeneralSettings> _adGeneralSettings)
    {
        

        public async Task Invoke(HttpContext context)
        {
            _logger.Debug("NtlmAndAnonymousSetupMiddleware - {RequestPath}", context.Request.Path);

            if(context?.User?.Identity?.IsAuthenticated ?? false)
            {
                await _next(context);
                return;
            }

            if ( _adGeneralSettings.Value.SupportSAML)
            {
                if (context.Request.Path.ToString().ToLower().StartsWith($"/signer", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.ToString().ToLower().StartsWith($"/saml", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

            }

            try
            {
                await context.ChallengeAsync("Windows");
            }
            catch
            {
                _logger.Warning("Failed to ChallengeAsync windows - need to set windows authentication or SAML  authentication ");
                await _next(context);
            }
        }
    }
}
