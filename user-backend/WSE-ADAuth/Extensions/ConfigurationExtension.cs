


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WSE_ADAuth.Models;
using Common.Models.Settings;
using Microsoft.AspNetCore.Server.IISIntegration;

namespace WSE_ADAuth.Extensions
{
    public static class ConfigurationExtension
    {
        public static void AddConfiguration(this IServiceCollection services, IConfiguration Configuration)
        {
            var adGeneralSettingsSection = Configuration.GetSection("ADGeneralSettings");
            var generalSettingsSection = Configuration.GetSection("GeneralSettings");
            var jwtSettingsSection = Configuration.GetSection("JwtSettings");
            var samlSettingsSection = Configuration.GetSection("SAMLGeneralSettings");
            var autoUserCreatingSettings = Configuration.GetSection("AutoUserCreatingSettings");



            services.Configure<JwtSettings>(jwtSettingsSection);
            services.Configure<ADGeneralSettings>(adGeneralSettingsSection);
            services.Configure<GeneralSettings>(generalSettingsSection);
            services.Configure<SAMLGeneralSettings>(samlSettingsSection);
            services.Configure<AutoUserCreatingSettings>(autoUserCreatingSettings);

            services.AddAuthentication(IISDefaults.AuthenticationScheme);
        }
    }
}
