using Common.Models.Settings;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;

namespace WeSignManagement.Helpers
{
    public class HangfireAuthorizationHelper : IDashboardAuthorizationFilter
    {
      


        public bool Authorize(DashboardContext context)
        {
            return true;
        }

    }
}
