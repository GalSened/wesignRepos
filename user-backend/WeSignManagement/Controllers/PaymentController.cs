using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using WeSignManagement.Models.Payment;
using Common.Models.ManagementApp;


using Common.Interfaces.ManagementApp;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/payment")]
#else
    [Route("v3/payment")]
#endif

    [Authorize(Roles = "PaymentAdmin")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class PaymentController : ControllerBase
    {
        private IManagementBL _bl;

        public PaymentController(IManagementBL bl)
            {
            _bl = bl;
        }

        /// <summary>
        /// Update user program after payment <br>
        /// Create new company and group for trail user or update documents limit for UserEmail <br>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UserPayment")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login(UserPaymentRequestDTO input)
        {
            var userPayment = new UserPayment()
            {
                UserEmail = input.UserEmail,
                ProgramID = input.ProgramID,
                MonthToAdd = input.MonthToAdd,
                ProgramResetType = input.ProgramResetType,
                TransactionId = input.PaymentTransactionId
                
            };
            await _bl.Payment.UserPay(userPayment);
            return Ok();
        }



        [HttpPost]
        [Route("UpdateRenwablePayment")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateRenwablePayment(PaymentRenewableRequestDTO input)
        {
            var userPayment = new UpdatePaymentRenewable()
            {
                Email = input.Email,
                OldTransactionId = input.OldTransactionId,
                TransactionId = input.TransactionId,               
            };
            await _bl.Payment.UpdateRenwablePayment(userPayment);
            return Ok();
        }



        [HttpPut]
        [Route("UnsubscribeCompany")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UnsubscribeCompany(UnsubscribeCompanyDTO input)
        {
            var company = new Company()
            {
                Id = input.CompanyId,                
            };
            await _bl.Payment.UnsubscribeCompany(company);
            return Ok();
        }


        [HttpPut]
        [Route("UpdateCompanyTransactionAndExpirationTime")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCompanyTransactionAndExpirationTime(UpdateCompanyTransactionAndExpirationTimeDTO input)
        {
            var company = new Company()
            {
                Id = input.CompanyId,
                ProgramUtilization = new Common.Models.Programs.ProgramUtilization() { Expired = input.NewExpirationTime },
                TransactionId = input.TransactionId


            };
            await _bl.Payment.UpdateCompanyTransactionAndExpirationTime(company);
            return Ok();
        }
    }
}
