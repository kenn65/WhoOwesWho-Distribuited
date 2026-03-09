using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.MessagingService.Services;
using WhoOwesWho.Shared.Models;


namespace WhoOwesWho.MessagingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController(IEmailMessagingService messagingService) : ControllerBase
    {
        [HttpPost]
        [Route("sendemail")]
        public async Task<IActionResult> SendEmail([FromBody] MessagingRequestModel request)
        {
            try
            {
                var response = await messagingService.SendEmailAsync(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while sending the email: {e.Message}");
            }
        }

    }
}

