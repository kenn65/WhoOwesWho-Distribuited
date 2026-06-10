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
    public class SettlementsController(IPaymentLookupService paymentLookupService) : ControllerBase
    {
        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]     
        public async Task<IActionResult> GetSettlementsAsync(Guid eventId, bool active)
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
        public async Task<IActionResult> GetSettlementAsync(Guid paymentId)
        {
            try
            {
                if (paymentId == Guid.Empty)
                {
                    return BadRequest(new PaymentDetailsPageResponseModel{
                         Message = Constants.RequestArgumentErrorMessages.PaymentIdArgumentError
                    });
                }

                var requestModel = new SettlementDetailsRequestModel
                {
                    PaymentId = paymentId,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentLookupService.GetSettlementDetailsAsync(requestModel));
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new PaymentDetailsPageResponseModel
                {
                    Message = e.Message
                });
                    
            }
        }
    }
}
