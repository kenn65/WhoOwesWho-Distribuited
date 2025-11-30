using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/events/{eventId}/users")]
    [ApiController]
    public class EventUsersController(IEventService eventService, IDataQueryService dataQueryService, IDataMutationService dataMutationService) : ControllerBase
    {


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEventUsersAsync(string eventId, [FromQuery] bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataQueryService.GetEventUsersAsync(eventId, token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> CheckUserAssignmentAsync(string eventId, string userId)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataQueryService.GetAssignmentAsync(userId!, token));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignUserAsync(string eventId, [FromBody] AssignmentRequestModel request)
        {
            try
            {
                request.EventId = eventId;
                request.Token = HttpContext.ToTokenValue();
                return Ok(await eventService.AssignAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpDelete("{userId}")]
        [Authorize]
        public async Task<IActionResult> UnassignUserAsync(string eventId, string userId)
        {
            try
            {
                var request = new UnassignmentRequestModel
                {
                    EventId = eventId,
                    UserId = userId,
                };
                return Ok(await dataMutationService.UnassignFromEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
