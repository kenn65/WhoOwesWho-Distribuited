using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserEventsController(
        IEventLookupService eventLookupService,
        IEventSecurityService eventSecurityService
        ) : ControllerBase
    {
        [HttpGet]
        [Route("{userId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetUserEventAsync(string userId, bool active = true)
        {

            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await eventLookupService.GetEventByUserAsync(userId, token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserEvents(string userId, [FromQuery] bool active = false)
        {
            try
            {
                if (active) { 

                }
                var token = HttpContext.ToTokenValue();
                var id = await eventSecurityService.UnprotectAsync(userId);
                var allEvents = (await eventLookupService.GetEventsAsync(token, active)).ToList();
                return Ok(allEvents.Where(e => e.Settled && e.Users!.Any(u => u.Id == Guid.Parse(id))));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


    }
}