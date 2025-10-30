using Common.Interfaces;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using OtpNet;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class OtpHandler : IOTP
    {
        private readonly string issuer = "WeSign";
        private readonly string label = "WeSign:systemAdmin@comda.co.il";
        private readonly GeneralSettings _generalSettings;

        public OtpHandler(IOptions<GeneralSettings> generalSettings)
        {
            _generalSettings = generalSettings.Value;
        }

        /// <summary>
        /// Create QR Code Image
        /// 
        /// </summary>
        /// <returns></returns>
        public string GenerateCode(Guid token = default, string identification = null)
        {
            string qrText = $"otpauth://totp/{label}?secret={_generalSettings.QRCodeSecret}&issuer={issuer}";
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        Bitmap qrCodeImage;
                        using (var ms = new MemoryStream(qrCode.GetGraphic(6)))
                        {
                            qrCodeImage = new Bitmap(ms);
                        }                    

                        return $"data:image/png;base64,{BitmapToBase64String(qrCodeImage)}";
                    }
                }
            }
        }

        public Task<(bool, Guid)> IsValidCode(string code,Guid token = default, bool incrementAttempts = false)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new Exception($"Null input - code is null or empty");
                }
                var totp = new Totp(Base32Encoding.ToBytes(_generalSettings.QRCodeSecret));
                var isValidTotpCode = totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
                return (isValidTotpCode, token);
            });  
        }

        public Task<(string,Guid)> ValidatePassword(Guid token = default, string identification = null, bool incrementAttempts = false)
        {
            throw new NotImplementedException();
        }

        #region Private Functions

        private string BitmapToBase64String(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img?.Save(stream, ImageFormat.Png);

                return Convert.ToBase64String(stream.ToArray());
            }
        }
        
        #endregion
    }
}
