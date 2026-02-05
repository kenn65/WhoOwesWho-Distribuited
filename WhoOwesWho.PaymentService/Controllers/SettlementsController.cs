using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.Models.Models.Extensions;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettlementsController(IPaymentService paymentService, IPaymentDetailsService paymentDetailsService) : ControllerBase
    {
        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]     
        public async Task<IActionResult> GetSettlementsAsync(string eventId, bool active)
        {
            try
            {
                var request = new PaymentsRequestModel
                {
                    EventId = eventId,
                    Active = active,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentService.GetPaymentsPageDataAsync(request));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }


        [HttpGet]
        [Route("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetSettlementAsync(string paymentId)
        {
            try
            {
                var request = new SettlementDetailsRequestModel
                {
                    PaymentId = paymentId,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentDetailsService.GetSettlementDetailsAsync(request));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
    }
}
