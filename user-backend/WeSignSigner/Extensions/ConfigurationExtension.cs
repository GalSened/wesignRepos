namespace WeSignSigner.Extensions
{
    using Common.Models.Settings;
    using Microsoft.Extensions.Configuration;
    using AspNetCoreRateLimit;
    using Microsoft.Extensions.DependencyInjection;

    public static class ConfigurationExtension
    {
        public static void AddConfiguration(this IServiceCollection services, IConfiguration Configuration)
        {
            var generalSettingsSection = Configuration.GetSection("GeneralSettings");
            var folderSettingsSection = Configuration.GetSection("FolderSettings");
            var jwtSettingsSection = Configuration.GetSection("JwtSettings");
            var IpRateLimitingSection = Configuration.GetSection("IpRateLimiting");
            var signerOneExtraInfoSettings = Configuration.GetSection("SignerOneExtraInfo");
            var rabbitMqSettings = Configuration.GetSection("RabbitMQSettings");
            var environmentExtraInfo = Configuration.GetSection("EnvironmentExtraInfo");

            services.Configure<GeneralSettings>(generalSettingsSection);
            services.Configure<FolderSettings>(folderSettingsSection);
            services.Configure<JwtSettings>(jwtSettingsSection);
            services.Configure<IpRateLimitOptions>(IpRateLimitingSection);
            services.Configure<SignerOneExtraInfoSettings>(signerOneExtraInfoSettings);
            services.Configure<RabbitMQSettings>(rabbitMqSettings);
            services.Configure<EnvironmentExtraInfo>(environmentExtraInfo);
        }
    }
}
