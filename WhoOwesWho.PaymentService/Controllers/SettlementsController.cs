using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettlementsController(IPaymentLookupService paymentLookupService) : ControllerBase
    {
        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]     
        public async Task<IActionResult> GetSettlementsAsync(string eventId, bool active)
        {
            try
            {
                var requestModel = new PaymentsRequestModel
                {
                    EventId = eventId,
                    Active = active
                    
                };
                return Ok(await paymentLookupService.GetPaymentsPageDataAsync(requestModel));
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
                var requestModel = new SettlementDetailsRequestModel
                {
                    PaymentId = paymentId,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentLookupService.GetSettlementDetailsAsync(requestModel));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
    }
}
