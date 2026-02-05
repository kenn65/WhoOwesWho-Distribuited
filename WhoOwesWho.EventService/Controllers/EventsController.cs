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
        [HttpPut]
        [Route("create")]
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
        [Route("{active}")]
        [Authorize]

        public async Task<IActionResult> GetEventsAsync(bool active)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventsAsync(token, active));
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
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventAsync(Guid.Parse(eventId), token, active));
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
                return Ok(await dataModificationService.UpdateEventAsync(request));
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
                return Ok(await dataModificationService.DeleteEventAsync(Guid.Parse(eventId)));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [Route("{eventId}/settle")]
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
