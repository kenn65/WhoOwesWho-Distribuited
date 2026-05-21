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
    public class EventUsersController(
        IEventLookupService eventLookupService, 
        IEventCommandService eventCommandService,
        EventAssignmentRequestValidator eventAssignmentValidator,
        EventUnassignmentRequestValidator eventUnassignmentValidator) : ControllerBase
    {
        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetEventUsersAsync(Guid eventId, bool active = true)
        {
            try
            {
                if (eventId == Guid.Empty)
                {
                    return BadRequest(new UserMessageResponseModel
                    {
                        Message = Constants.EventErrorMessages.EventIdMissing
                    });
                }
                var response = await eventLookupService.GetEventUsersAsync(eventId, active);
                return Ok(new EnumerableWrapperResponseModel<IEnumerable<UserMessageResponseModel>>
                {
                     Data = response
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserMessageResponseModel
                {
                    Message = e.Message
                });
            }
        }


        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserAssignmentAsync(Guid userId, [FromQuery] bool active)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new EventAssignmentModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                return Ok(await eventLookupService.GetAssignmentAsync(userId!, active));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new EventAssignmentModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("assign")]
        [Authorize]
        public async Task<IActionResult> AssignUserAsync([FromBody] AssignmentRequestModel request)
        {
            try
            {
                var validationResult = await eventAssignmentValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new AssignmentResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }

                return Ok(await eventCommandService.AssignAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AssignmentResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("unassign")]
        [Authorize]
        public async Task<IActionResult> UnassignUserAsync([FromBody] UnassignmentRequestModel request)
        {
            try
            {
                var validationResult = await eventUnassignmentValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new UnassignmentResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await eventCommandService.UnassignFromEventAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UnassignmentResponseModel
                {
                    Message = e.Message
                });
            }
        }
    }
}
