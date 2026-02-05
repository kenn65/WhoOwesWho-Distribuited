using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventUsersController(IEventService eventService, IDataQueryService dataQueryService, IDataMutationService dataMutationService) : ControllerBase
    {
        [HttpGet]
        [Route("{userId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetEventUsersAsync(string userId, bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataQueryService.GetEventUsersAsync(userId, token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> CheckUserAssignmentAsync(string userId)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataQueryService.GetUserAssignmentAsync(userId, token, true));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpPost]
        [Route("assign")]
        [Authorize]
        public async Task<IActionResult> AssignUserAsync([FromBody] AssignmentRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await eventService.AssignAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpPost]
        [Route("unassign")]
        [Authorize]
        public async Task<IActionResult> UnassignUserAsync([FromBody] UnassignmentRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await dataMutationService.UnassignFromEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
