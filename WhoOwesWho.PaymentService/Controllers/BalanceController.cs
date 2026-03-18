using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController(
        IUserBalanceService userBalanceService 
        ) : ControllerBase
    {
        [HttpGet]
        [Route("{userId}/{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetUserBalanceAsync(string userId, string eventId)
        {
            try
            {
                var requestModel = new UserBalanceRequestModel
                {
                    UserId = userId,
                    EventId = eventId,
                    Token = HttpContext.ToTokenValue()
                };

                var response = await userBalanceService.GetUserBalanceAsync(requestModel, true);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
