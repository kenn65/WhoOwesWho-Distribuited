using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(IEventCommandService eventCommanddService, IEventLookupService eventLookupService) : ControllerBase
    {
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> CreateEventAsync([FromBody] EventRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await eventCommanddService.CreateEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("{active}")]
        [Authorize]

        public async Task<IActionResult> GetEventsAsync(bool active)
        {
            try
            {
                return Ok(await eventLookupService.GetEventsAsync(active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpGet("{eventId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetEventAsync(string eventId, bool active)
        {
            try
            {
                return Ok(await eventLookupService.GetEventAsync(eventId, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
             

        [HttpPatch]
        [Route("update")]
        [Authorize]
        public async Task<IActionResult> UpdateEventAsync([FromBody] EventRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await eventCommanddService.UpdateEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpDelete]
        [Route("{eventId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEventAsync(string eventId)
        {
            try
            {
                return Ok(await eventCommanddService.DeleteEventAsync(eventId));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpPost]
        [Route("settle")]
        [Authorize]
        public async Task<IActionResult> SettleEventAsync([FromBody] SettleEventRequestModel request)
        {
            try
            {
                var response = await eventCommanddService.SettleEventAsync(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
