using Common.Interfaces.Dashboard;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;

namespace WeSign.areas.ui.Controllers
{
//#if DEBUG
//    [Route("userui/v3/dashboard")]
//#else
//    [Route("ui/v3/dashboard")]
//#endif
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboard _dashboard;

        public DashboardController(IDashboard dashboard)
        {
            _dashboard = dashboard;
        }

        /// <summary>
        /// Read dashboard view
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("view")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDashboardView()
        {
            var dashboardView = await _dashboard.GetDashboardView();
            if (dashboardView == null) 
            {
                return NoContent();
            }
            return Ok(dashboardView);
        }
    }
}
