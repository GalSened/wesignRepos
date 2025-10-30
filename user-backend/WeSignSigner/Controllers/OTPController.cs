using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeSignSigner.Models.Requests;
using WeSignSigner.Models.Responses;

namespace WeSignSigner.Controllers
{
#if DEBUG
    [Route("signerapi/v3/otp")]
#else
    [Route("v3/otp")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class OTPController : ControllerBase
    {
        private readonly IOTP _otp;

        public OTPController(IOTP otp)
        {
            _otp = otp;
        }

        /// <summary>
        /// Validate Password and according to OTP mode generate OTP code and send it to signer phone/mail
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GenerateCodeResponseDTO))]
        public async Task<IActionResult> GenerateCode(GenerateCodeDTO input)
        {
            (string sentSignerMeans,var authToken) =  await _otp.ValidatePassword(input.Token, input.Identification, true);

            string encryptSentSignerMeans = sentSignerMeans == string.Empty ? string.Empty : sentSignerMeans.Contains("@") ?
                                            $"{sentSignerMeans.Substring(0, 2)}****{sentSignerMeans.Substring(sentSignerMeans.IndexOf("@"), 3)}*****{sentSignerMeans.Substring(sentSignerMeans.Length - 2, 2)}" :
                                            $"{sentSignerMeans.Substring(0, 6)}*****{sentSignerMeans.Substring(sentSignerMeans.Length - 2, 2)}";

            GenerateCodeResponseDTO responseDTO = new GenerateCodeResponseDTO { SentSignerMeans = encryptSentSignerMeans };
            if (string.IsNullOrWhiteSpace(encryptSentSignerMeans))
            {
                responseDTO.AuthToken = authToken;
            }

            return Ok(responseDTO);
        }

        /// <summary>
        /// Check if OTP code is valid 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GenerateCodeResponseDTO))]
        public async Task<IActionResult> IsValidCode(Guid token, string code)
        {
            (bool isValidOTPCode, var authToken) = await _otp.IsValidCode(code, token, true);
            GenerateCodeResponseDTO responseDTO = new GenerateCodeResponseDTO { AuthToken = authToken };
            return isValidOTPCode ? Ok(responseDTO) : (IActionResult)BadRequest();
        }
    }
}