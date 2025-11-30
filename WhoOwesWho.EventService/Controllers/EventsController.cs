using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(
        IDataMutationService dataModificationService, 
        IDataQueryService dataSelectionService)
        : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEventAsync([FromBody] EventRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await dataModificationService.CreateEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEventsAsync([FromQuery] bool active)
        {
            try
            {
                return Ok(await dataSelectionService.GetEventsAsync(active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpGet("{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetEventAsync( string eventId, [FromQuery] bool active)
        {
            try
            {
                return Ok(await dataSelectionService.GetEventAsync(Guid.Parse(eventId), active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
             

        [HttpPut("{eventId}")]
        [Authorize]
        public async Task<IActionResult> UpdateEventAsync(string eventId, [FromBody] EventRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                request.Id = Guid.Parse(eventId);
                return Ok(await dataModificationService.UpdateEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpDelete("{eventId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEventAsync(string eventId)
        {
            try
            {
                return Ok(await dataModificationService.DeleteEventAsync(Guid.Parse(eventId)));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpPost("{eventId}/settle")]
        [Authorize]
        public async Task<IActionResult> SettleEventAsync(string eventId)
        {
            try
            {
                var request = new SettleEventRequestModel
                {
                    EventId = eventId
                };
                var response = await dataModificationService.SettleEventAsync(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
