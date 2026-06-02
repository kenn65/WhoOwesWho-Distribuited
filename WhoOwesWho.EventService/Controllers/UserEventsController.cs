using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserEventsController(
        IEventLookupService eventLookupService
        ) : ControllerBase
    {
        [HttpGet]
        [Route("{userId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetUserEventAsync(Guid userId, bool active = true)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new EventResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                return Ok(await eventLookupService.GetEventByUserAsync(userId, active));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new EventResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserEvents(Guid userId, [FromQuery] bool active = false)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new EventResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                var allEvents = (await eventLookupService.GetEventsAsync(active)).ToList();
                var response = allEvents.Where(e => e.Settled == !active && e.Users!.Any(u => u.Id == userId));
                return Ok(new EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>
                {
                    Data = response,
                    Success = true
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new EventResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserEvents([FromQuery] Guid userId)
        {
            try
            {
               if (userId == Guid.Empty)
                {
                    return BadRequest(new EventResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
               var response = await eventLookupService.GetEventsAsync(userId);
                return Ok(new EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>
                {
                    Data = response,
                    Success = true
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new EventResponseModel
                {
                    Message = e.Message
                });
            }
        }
    }
}