using Common.Interfaces.ManagementApp;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class LicenseDMZHandler : ILicenseDMZ
    {
        private GeneralSettings _generalSettings;

        public LicenseDMZHandler(IOptions<GeneralSettings> generalSettings)
        {
            _generalSettings = generalSettings.Value;
        }
        public async Task<bool> IsDMZReachable()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var result = await client.GetAsync($"{_generalSettings.LicenseDMZEndpoint}/requests");
                    return result.StatusCode == HttpStatusCode.OK;
                
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
