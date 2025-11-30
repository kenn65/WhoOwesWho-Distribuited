using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.Models.Models.Extensions;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(
        IPaymentService paymentService, 
        IPaymentDetailsService paymentDetailsService,
        IUnprotectValueMessageSender unprotectValueMessageSender) : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            try
            {
                request.CreditorId = await unprotectValueMessageSender.SendAsync(request.CreditorId!);
                
                var userIdList = request.UserIds!.ToList();
                var unprotectedUserIds = new List<string>();
                for (var i = userIdList.Count - 1; i > -1; i--)
                {
                    var userId = await unprotectValueMessageSender.SendAsync(userIdList[i]);
                    unprotectedUserIds.Add(userId);
                }
                request.UserIds = unprotectedUserIds;
                var response = await paymentService.CreatePaymentAsync(request);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPaymentsAsync([FromQuery] string userId, [FromQuery] bool active)
        {
            try
            {
                var request = new PaymentsRequestModel
                {
                    UserId = userId,
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


        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentAsync([Required] string paymentId)
        {
            try
            {
                var request = new PaymentDetailsPageRequestModel
                {
                    PaymentId = paymentId,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentDetailsService.GetPaymentDetailsAsync(request));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
        
        [HttpPut("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePaymentAsync(string paymentId, [FromBody] UpdatePaymentRequestModel request)
        {
            try
            {
                request.PaymentId = Guid.Parse(paymentId);
                request.CreditorId = await unprotectValueMessageSender.SendAsync(request.CreditorId!);
                
                var userIdList = request.UserIds!.ToList();
                var unprotectedUserIds = new List<string>();
                for (var i = userIdList.Count - 1; i > -1; i--)
                {
                    var userId = await unprotectValueMessageSender.SendAsync(userIdList[i]);
                    unprotectedUserIds.Add(userId);
                }
                request.UserIds = unprotectedUserIds;
                var response = await paymentDetailsService.UpdatePaymentDetailsAsync(request);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> RemovePaymentAsync(string paymentId)
        {
            try
            {
                return Ok(await paymentDetailsService.DeletePaymentAsync(new DeletePaymentRequestModel
                {
                    PaymentId = paymentId
                }));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
    }
}
