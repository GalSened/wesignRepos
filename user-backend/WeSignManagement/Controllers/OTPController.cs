using System.Net;
using System.Threading.Tasks;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/otp")]
#else
    [Route("v3/otp")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class OTPController : Controller
    {
        private readonly IManagementBL _bl;
        public OTPController(IManagementBL bl)
        {
            _bl = bl;
        }

        [Authorize(Roles = "Ghost")]
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(QRCodeResponseDTO))]
        public IActionResult CreateQRCode()
        {
            string image = _bl.OTP.GenerateCode();
            return Ok(new QRCodeResponseDTO
            {
                Image = image
            });
        }

        [HttpGet]
        [Route("verify")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(QRCodeVerificationResponseDTO))]
        public async Task<IActionResult> VerifyCode(string code)
        {
            bool isValid = (await _bl.OTP.IsValidCode(code)).Item1;

            return Ok(new QRCodeVerificationResponseDTO
            {
                IsValid = isValid
            });
        }
    }
}