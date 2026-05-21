using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Validators;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(
        IPaymentLookupService paymentLookupService,
        IPaymentCommandService paymentCommandService,
        CreatePaymentRequestValidator createPaymentRequestValidator,
        UpdatePaymentRequestValidator updatePaymentRequestValidator
        ) : ControllerBase
    {
        [HttpPut]
        [Route("create")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            try
            {
                var validationResult = createPaymentRequestValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new CreatePaymentResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await paymentCommandService.CreatePaymentAsync(request));
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new CreatePaymentResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("{eventId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentsAsync(Guid eventId, bool active)
        {
            try
            {
                if (eventId == Guid.Empty)
                {
                    return BadRequest(new PaymentPageResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.EventIdArgumentError
                    });
                }
                var requestModel = new PaymentsRequestModel
                {
                    EventId = eventId,
                    Active = active
                };
                return Ok(await paymentLookupService.GetPaymentsPageDataAsync(requestModel));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new PaymentPageResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("{eventId}/{userId}/{active}")]
        [Authorize]
        public async Task<IActionResult> GetUserPaymentsAsync(Guid eventId, Guid userId, bool active)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new UserBalanceResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                if (eventId == Guid.Empty)
                {
                    {
                        return BadRequest(new UserBalanceResponseModel
                        {
                            Message = Constants.RequestArgumentErrorMessages.EventIdArgumentError
                        });
                    }
                }
                var requestModel = new UserPaymentsRequestModel
                {
                    EventId = eventId,
                    UserId = userId,
                    Active = active
                };
                return Ok(await paymentLookupService.GetUserPaymentsAsync(requestModel));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserPaymentResponseModel
                {
                    Message = e.Message
                });
            }
        }


        [HttpGet]
        [Route("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentAsync(Guid paymentId, [FromQuery] bool active)
        {
            try
            {
                if (paymentId == Guid.Empty)
                {
                    return BadRequest(new PaymentDetailsPageResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.PaymentIdArgumentError
                    });
                }

                var requestModel = new PaymentDetailsPageRequestModel
                {
                    PaymentId = paymentId,
                    Active = active,
                    Token = HttpContext.ToTokenValue()
                };
                
                return Ok(await paymentLookupService.GetPaymentDetailsAsync(requestModel));
            } 
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new PaymentDetailsPageResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPatch]
        [Route("update")]
        [Authorize]
        public async Task<IActionResult> UpdatePaymentAsync([FromBody] UpdatePaymentRequestModel request)
        {
            try
            {
                var validationResult = updatePaymentRequestValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new UpdatePaymentResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }

                var response = await paymentCommandService.UpdatePaymentDetailsAsync(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdatePaymentResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpDelete]
        [Route("delete/{paymentId}")]
        [Authorize]
        public async Task<IActionResult> RemovePaymentAsync(Guid paymentId)
        {
            try
            {
                if (paymentId == Guid.Empty)
                {
                    return BadRequest(new DeletePaymentResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.PaymentIdArgumentError
                    });
                }
                return Ok(await paymentCommandService.DeletePaymentAsync(paymentId));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new DeletePaymentResponseModel
                    {
                        Message = e.Message
                    });
            }
        }
    }
}
