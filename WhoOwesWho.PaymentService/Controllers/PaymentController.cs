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
    public class PaymentController(
        IPaymentService paymentService, 
        IUserBalanceService userBalanceService, 
        IPaymentDetailsService paymentDetailsService,
        IUnprotectValueMessageSender unprotectValueMessageSender) : ControllerBase
    {
        [HttpPost]
        [Authorize]
        [Route("single/create")]
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
        [Route("single/balance")]
        public async Task<IActionResult> GetUserBalanceAsync([Required] string userId, [Required] string eventId)
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

                //var request = new UserBalanceRequestModel
                //{
                //    UserId = await encryptionGatewayService.UnprotectAsync(userId),
                //    EventId = eventId,
                //    Token = HttpContext.ToTokenValue()
                //};

                var response = await userBalanceService.GetUserBalanceAsync(request, true);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("payments")]
        [ResponseCache(Location = ResponseCacheLocation.None, Duration = 0, NoStore = true)]
        public async Task<IActionResult> GetPaymentsAsync([Required] string userId, [Required] bool active)
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

        [HttpGet]
        [Authorize]
        [Route("settlements")]

        public async Task<IActionResult> GetSettlementsAsync([Required] string eventId, [Required] bool active)
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
        [Authorize]
        [Route("payments/refresh")]
        public async Task<IActionResult> GetPaymentsRefreshAsync([Required] string userId)
        {
            try
            {
                var request = new PaymentsRequestModel
                {
                    UserId = userId,
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
        [Authorize]
        [Route("single/get")]
        public async Task<IActionResult> GetPaymentDetailsAsync([Required] string paymentId)
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

        [HttpGet]
        [Authorize]
        [Route("settlements/single/get")]
        public async Task<IActionResult> GetSettlementDetailsAsync([Required] string paymentId)
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

        [HttpPost]
        [Authorize]
        [Route("single/update")]
        public async Task<IActionResult> UpdatePaymentAsync(UpdatePaymentRequestModel request)
        {
            try
            {
                request.CreditorId = await unprotectValueMessageSender.SendAsync(request.CreditorId!);
                
                //request.CreditorId = await encryptionGatewayService.UnprotectAsync(request.CreditorId!);

                var userIdList = request.UserIds!.ToList();
                var unprotectedUserIds = new List<string>();
                for (var i = userIdList.Count - 1; i > -1; i--)
                {
                    var userId = await unprotectValueMessageSender.SendAsync(userIdList[i]);
                    unprotectedUserIds.Add(userId);

                    //var userId = userIdList[i];
                    //unprotectedUserIds.Add(await encryptionGatewayService.UnprotectAsync(userId));
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

        [HttpPost]
        [Authorize]
        [Route("single/delete")]
        public async Task<IActionResult> RemovePaymentAsync(DeletePaymentRequestModel request)
        {
            try
            {
                return Ok(await paymentDetailsService.DeletePaymentAsync(request));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
    }
}
