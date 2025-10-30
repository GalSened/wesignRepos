namespace HistoryIntegratorService.Common.Models
{
    public class JwtSettings
    {
        public string JwtBearerSignatureKey { get; set; }
        public string RefreshTokenExpirationInMinutes { get; set; }
        public int SessionExpireMinuteTime { get; set; }
    }
}
