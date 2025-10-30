namespace Common.Models.Settings
{
    public class JwtSettings
    {
        public string JwtBearerSignatureKey { get; set; }
        public string RefreshTokenExpirationInMinutes { get; set; }
        public int SessionExpireMinuteTime { get; set; }
        public string JwtSignerSignatureKey { get; set; }
        public int SignerLinkExpirationInHours { get; set; }
    }
}
