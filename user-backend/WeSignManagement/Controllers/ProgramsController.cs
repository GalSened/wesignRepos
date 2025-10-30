using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.License;
using Common.Models.ManagementApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeSignManagement.Models.Programs;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/programs")]
#else
    [Route("v3/programs")]
#endif

    [Authorize(Roles = "SystemAdmin")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]

    public class ProgramsController : ControllerBase
    {
        private readonly IManagementBL _bl;

        public ProgramsController(IManagementBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Common.Models.Program))]
        public async Task<IActionResult> Read(string id)
        {
            Common.Models.Program program = new Common.Models.Program
            {
                Id = new Guid(id)
            };

            program =await _bl.Programs.Read(program);

            return Ok(program);
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllProgramsResponseDTO))]
        public IActionResult Read(string key = null, int offset = 0, int limit = 20)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < -1)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            
            IEnumerable<Common.Models.Program> programs = _bl.Programs.Read(key, offset, limit, out int totalCount);

            var response = new AllProgramsResponseDTO()
            {
                Programs = programs.ToList()
            };

            Response.Headers.Add("x-total-count", totalCount.ToString());


            return Ok(response);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(Guid id ,ProgramDTO input)
        {
            Common.Models.Program program = new Common.Models.Program()
            {
                Id = id,
                Name = input.Name,
                Users = input.Users,
                Templates = input.Templates,
                DocumentsPerMonth = input.DocumentsPerMonth,
                SmsPerMonth = input.SmsPerMonth,
                VisualIdentificationsPerMonth = input.VisualIdentificationsPerMonth,
                VideoConferencePerMonth = input.VideoConferencePerMonth,
                ServerSignature = input.ServerSignature,
                SmartCard = input.SmartCard,
                Note = input.Note,
                UIViewLicense = new UIViewLicense(input.UIViewLicense)
            };
            await _bl.Programs.Update(program);

            return Ok();
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(ProgramDTO input)
        {
            Common.Models.Program program = new Common.Models.Program()
            {
                Name = input.Name,
                Users = input.Users,
                Templates = input.Templates,
                DocumentsPerMonth = input.DocumentsPerMonth,
                SmsPerMonth = input.SmsPerMonth,
                VisualIdentificationsPerMonth = input.VisualIdentificationsPerMonth,
                VideoConferencePerMonth = input.VideoConferencePerMonth,
                ServerSignature = input.ServerSignature,
                SmartCard = input.SmartCard,
                Note = input.Note,
                UIViewLicense = new UIViewLicense(input.UIViewLicense)
            };
            await _bl.Programs.Create(program);

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            Common.Models.Program program = new Common.Models.Program()
            {
                Id = id
            };
            await _bl.Programs.Delete(program);

            return Ok();
        }

    }
}