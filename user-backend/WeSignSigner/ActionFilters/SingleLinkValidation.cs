using Common.Models.Settings;
using DAL.Migrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace WeSignSigner.ActionFilters
{
    public class SingleLinkValidation : IActionFilter
    {
        private readonly GeneralSettings _settings;

        public SingleLinkValidation(IOptions<GeneralSettings> options)
        {
            
            _settings = options.Value;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_settings.AllowSingleLink)
            {
                context.Result = new BadRequestObjectResult(new { Message= "SingleLink is not allowed" });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }


    }
}
