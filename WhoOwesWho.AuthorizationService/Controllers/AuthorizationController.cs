using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Encryption;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(
        IConfiguration configuration,
        IAuthorizationService authorizationService, 
        IAuthenticationService authenticationService, 
        IValidationService validationService, 
        IUnprotectValueMessageSender unprotectMessageSender
         ) : ControllerBase
    {
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequestModel request)
        {
            var actionResult = new AuthenticationResponseModel();
            
            try
            {
                request.Password = await unprotectMessageSender.SendAsync(new UnprotectValueRequestModel
                {
                    ApiKey = configuration["EncryptionMicroService:Security:ApiKey"]!,
                    Text = request.Password!
                });
                
                if (string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.Password))
                {
                    actionResult.Message = "E-mail address or password was not provided";
                    return Ok(actionResult);
                }

                if (!await validationService.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                {
                    actionResult.Message = "Invalid combination of e-mail and password entered.";
                    return Ok(actionResult);
                }

                var code = await authenticationService.SendAuthenticationMessage(request);
                actionResult.Success = !string.IsNullOrWhiteSpace(code);
                actionResult.Code = code;
                actionResult.Message = actionResult.Success
                    ? "An authentication code was sent to your e-mail address"
                    : "An unexpected error occurred, please try again.";
                return Ok(actionResult);
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpPost]
        [Route("authorize")]
        public async Task<IActionResult> Authorize([FromBody] AuthorizationRequestModel request)
        {
            try
            {
                return Ok(await authorizationService.Authorize(request));
                
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
