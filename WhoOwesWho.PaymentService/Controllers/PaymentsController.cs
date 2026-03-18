using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(
        IPaymentLookupService paymentLookupService,
        IPaymentCommandService paymentCommandService
        ) : ControllerBase
    {
        [HttpPut]
        [Route("create")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            try
            {
                return Ok(await paymentCommandService.CreatePaymentAsync(request));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentsAsync(string eventId, bool active)
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
        [Route("{eventId}/{userId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetUserPaymentsAsync(string eventId, string userId, bool active)
        {
            try
            {
                var requestModel = new UserPaymentsRequestModel
                {
                    EventId = eventId,
                    UserId = userId,
                    Active = active
                };
                return Ok(await paymentLookupService.GetUserPaymentsAsync(requestModel));
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
        public async Task<IActionResult> GetPaymentAsync(string paymentId)
        {
            try
            {
                var requestModel = new PaymentDetailsPageRequestModel
                {
                    PaymentId = paymentId,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentLookupService.GetPaymentDetailsAsync(requestModel));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [HttpPatch]
        [Route("update")]
        [Authorize]
        public async Task<IActionResult> UpdatePaymentAsync([FromBody] UpdatePaymentRequestModel request)
        {
            try
            {
                var response = await paymentCommandService.UpdatePaymentDetailsAsync(request);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [HttpDelete]
        [Route("delete/{paymentId}")]
        [Authorize]
        public async Task<IActionResult> RemovePaymentAsync(string paymentId)
        {
            try
            {
                return Ok(await paymentCommandService.DeletePaymentAsync(paymentId));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
    }
}
