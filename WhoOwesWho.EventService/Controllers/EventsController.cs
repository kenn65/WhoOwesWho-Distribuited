using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Validators;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(
        IEventCommandService eventCommanddService, 
        IEventLookupService eventLookupService,
        CreateEventRequestValidator createEventValidator,
        UpdateEventRequestValidator updateEventValidator
        ) : ControllerBase
    {
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> CreateEventAsync([FromBody] EventRequestModel request)
        {
            try
            {
                var validationResult = await createEventValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new EventResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await eventCommanddService.CreateEventAsync(request));
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
        [Route("{active}")]
        [Authorize]
        public async Task<IActionResult> GetEventsAsync(bool active)
        {
            try
            {
                var response = await eventLookupService.GetEventsAsync(active);
                return Ok(new EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>
                {
                    Data = response
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


        [HttpGet("{eventId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetEventAsync(Guid eventId, bool active)
        {
            try
            {
                if (eventId == Guid.Empty)
                {
                    return BadRequest(new EventResponseModel
                    {
                        Message = Constants.EventErrorMessages.EventIdMissing
                    });
                }
                return Ok(await eventLookupService.GetEventAsync(eventId, active));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new EventResponseModel
                {
                    Message = e.Message
                });
            }
        }
             

        [HttpPatch]
        [Route("update")]
        [Authorize]
        public async Task<IActionResult> UpdateEventAsync([FromBody] EventRequestModel request)
        {
            try
            {
                var validationResult = await updateEventValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new UpdateResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await eventCommanddService.UpdateEventAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdateResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpDelete]
        [Route("{eventId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEventAsync(Guid eventId)
        {
            try
            {
                if (eventId == Guid.Empty)
                {
                    return BadRequest(new DeleteEventResponseModel
                    {
                        Message = Constants.EventErrorMessages.EventIdMissing
                    });
                }
                return Ok(await eventCommanddService.DeleteEventAsync(eventId));

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DeleteEventResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("settle")]
        [Authorize]
        public async Task<IActionResult> SettleEventAsync([FromBody] SettleEventRequestModel request)
        {
            try
            {
                if (request.EventId == Guid.Empty)
                {
                    return BadRequest(new SettleEventResponseModel
                    {
                        Message = Constants.EventErrorMessages.EventIdMissing
                    });
                }
                var response = await eventCommanddService.SettleEventAsync(request.EventId);
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new SettleEventResponseModel
                {
                    Message = e.Message
                });
            }
        }
    }
}
