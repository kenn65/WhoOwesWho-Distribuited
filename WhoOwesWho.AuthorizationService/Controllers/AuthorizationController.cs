using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(
        IAuthorizationService authorizationService, 
        IAuthenticationNotificationService authenticationNotificationService
        ) : ControllerBase
    {
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequestModel request)
        {
            try
            {
                return Ok(await authenticationNotificationService.SendAuthenticationMessage(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
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
                return BadRequest(e.Message);
            }
        }
    }
}
