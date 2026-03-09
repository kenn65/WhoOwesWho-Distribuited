using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventUsersController(IEventLookupService eventLookupService, IEventCommandService eventCommandService) : ControllerBase
    {
        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetEventUsersAsync(string eventId, bool active = true)
        {
            try
            {
                return Ok(await eventLookupService.GetEventUsersAsync(eventId, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserAssignmentAsync(string userId)
        {
            try
            {
                return Ok(await eventLookupService.GetAssignmentAsync(userId!, true));
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
                return Ok(await eventCommandService.AssignAsync(request));
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
                return Ok(await eventCommandService.UnassignFromEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
