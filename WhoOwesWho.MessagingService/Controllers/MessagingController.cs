using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.MessagingService.Services;
using WhoOwesWho.MessagingService.Validators;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.MessagingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController(IEmailMessagingService messagingService, MessagingRequestValidator validator) : ControllerBase
    {
        [HttpPost]
        [Route("sendemail")]
        public async Task<IActionResult> SendEmail([FromBody] MessagingRequestModel request)
        {
            try
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new MessagingResponseErrorMessagesModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }

                var response = await messagingService.SendEmailAsync(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new MessagingResponseErrorMessagesModel
                {
                    Message = e.Message
                });
            }
        }

    }
}

