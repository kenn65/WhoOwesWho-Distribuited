using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Models.Models.Extensions;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController(IUserBalanceService userBalanceService, IUnprotectValueMessageSender unprotectValueMessageSender) : ControllerBase
    {
        [HttpGet("{userId}/balance")]
        [Authorize]
        public async Task<IActionResult> GetUserBalanceAsync(string userId, [FromQuery] string eventId)
        {
            try
            {
                var unprotectedUserId = await unprotectValueMessageSender.SendAsync(userId);

                var request = new UserBalanceRequestModel
                {
                    UserId = unprotectedUserId,
                    EventId = eventId,
                    Token = HttpContext.ToTokenValue()
                };

                var response = await userBalanceService.GetUserBalanceAsync(request, true);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
