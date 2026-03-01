using System.ComponentModel.DataAnnotations;
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
    public class PaymentsController(
        IPaymentLookupService paymentLookupService, 
        IPaymentCommandService paymentCommandService,
        IEncryptionGatewayService encryptionGatewayService
        ) : ControllerBase
    {
        [HttpPut]
        [Route("create")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            try
            {
                request.CreditorId = await encryptionGatewayService.UnprotectAsync(request.CreditorId!);
                                
                var userIdList = request.UserIds!.ToList();
                var unprotectedUserIds = new List<string>();
                for (var i = userIdList.Count - 1; i > -1; i--)
                {
                    var userId = await encryptionGatewayService.UnprotectAsync(userIdList[i]);
                    unprotectedUserIds.Add(userId);
                }
                request.UserIds = unprotectedUserIds;
                var response = await paymentCommandService.CreatePaymentAsync(request);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


        [HttpGet]
        [Route("{userId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentsAsync(string userId, bool active)
        {
            try
            {
                var request = new PaymentsRequestModel
                {
                    UserId = userId,
                    Active = active,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentLookupService.GetPaymentsPageDataAsync(request));
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
                var request = new PaymentDetailsPageRequestModel
                {
                    PaymentId = paymentId,
                    Token = HttpContext.ToTokenValue()
                };
                return Ok(await paymentLookupService.GetPaymentDetailsAsync(request));
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
                request.CreditorId = await encryptionGatewayService.UnprotectAsync(request.CreditorId!);
                
                var userIdList = request.UserIds!.ToList();
                var unprotectedUserIds = new List<string>();
                for (var i = userIdList.Count - 1; i > -1; i--)
                {
                    var userId = await encryptionGatewayService.UnprotectAsync(userIdList[i]);
                    unprotectedUserIds.Add(userId);
                }
                request.UserIds = unprotectedUserIds;
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
