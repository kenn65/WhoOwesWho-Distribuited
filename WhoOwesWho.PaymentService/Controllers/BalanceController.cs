using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.Models.Models.Extensions;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController(
        IUserBalanceService userBalanceService, 
        IEncryptionGatewayService encryptionGatewayService
        ) : ControllerBase
    {
        [HttpGet]
        [Route("{userId}/{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetUserBalanceAsync(string userId, string eventId)
        {
            try
            {
                var unprotectedUserId = await encryptionGatewayService.UnprotectAsync(userId);
                
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
