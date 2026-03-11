using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services;

namespace WhoOwesWho.AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(
        IAuthorizationService authorizationService, 
        IAuthenticationNotificationService authenticationNotificationService, 
        IAuthenticationValidationService authenticationValidationService 
        ) : ControllerBase
    {
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequestModel request)
        {
            var actionResult = new AuthenticationResponseModel();
            
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.Password))
                {
                    actionResult.Message = "E-mail address or password was not provided";
                    return Ok(actionResult);
                }

                if (!await authenticationValidationService.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                {
                    actionResult.Message = "Invalid e-mail and/or password entered.";
                    return Ok(actionResult);
                }

                var code = await authenticationNotificationService.SendAuthenticationMessage(request);
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
