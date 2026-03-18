using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.Shared.Models;

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
                return Ok(await eventLookupService.GetEventByUserAsync(userId, active));
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
                try
                {
                    var id = await eventSecurityService.UnprotectAsync(userId);
                    var allEvents = (await eventLookupService.GetEventsAsync(active)).ToList();
                    return Ok(allEvents.Where(e => e.Settled == !active && e.Users!.Any(u => u.Id == Guid.Parse(id))));
                }
                catch (Exception e)
                {
                    return BadRequest(e.StackTrace);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


    }
}