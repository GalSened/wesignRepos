using Common.Interfaces;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BL.Handlers
{
    public class UserHangfireHandler : IUserHangfire
    {
        private ILogger _logger;
        private GeneralSettings _generalSettings;

        public UserHangfireHandler(ILogger logger, IOptions<GeneralSettings> generalSettings)
        {
            _logger = logger;
            _generalSettings = generalSettings.Value;
        }
        public void WakeUpManagementHangfire()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_generalSettings.ManagementAPIUrl))
                {
                    using (var client = new HttpClient())
                    {
                        var result =  client.GetAsync($"{_generalSettings.ManagementAPIUrl}/jobs").Result;
                    }
                }
                else
                {
                    _logger.Warning("Management Api URL in general settings is missing");
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to check Management Hangfile Jobs");
            }

        }
    }
}
