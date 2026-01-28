using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(
        IAuthorizationService authorizationService, 
        IAuthenticationService authenticationService, 
        IValidationService validationService, 
        IEncryptionGatewayService encryptionGatewayService
        ) : ControllerBase
    {
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequestModel request)
        {
            var actionResult = new AuthenticationResponseModel();
            
            try
            {
                request.Password = await encryptionGatewayService.UnprotectAsync(request.Password!, false);

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
