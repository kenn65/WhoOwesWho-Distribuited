using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController(IUserBalanceService userBalanceService ) : ControllerBase
    {
        [HttpGet]
        [Route("{userId}/{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetUserBalanceAsync(Guid userId, Guid eventId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new UserBalanceResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                if (eventId == Guid.Empty)
                {
                    {
                        return BadRequest(new UserBalanceResponseModel
                        {
                            Message = Constants.RequestArgumentErrorMessages.EventIdArgumentError
                        });
                    }
                }
                var requestModel = new UserBalanceRequestModel
                {
                    UserId = userId,
                    EventId = eventId,
                };                    
                var response = await userBalanceService.GetUserBalanceAsync(requestModel, true);
                return Ok(response);
            }
            catch (Exception e) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserBalanceResponseModel
                {
                    Message = e.Message
                });
            }
        }
    }
}
