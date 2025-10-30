using HistoryIntegratorService.Common.Models;

namespace HistoryIntegratorService.Extensions
{
    public static class ConfigurationExtension
    {
        public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var generalSettingsSection = configuration.GetSection(nameof(GeneralSettings));
            services.Configure<GeneralSettings>(generalSettingsSection);
        }
    }
}
