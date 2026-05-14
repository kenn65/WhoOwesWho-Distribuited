using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Validators;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(
        IAuthorizationService authorizationService,
        IAuthenticationNotificationService authenticationNotificationService,
        IAuthorizationSecurityService authorizationSecurityService,
        AuthenticationRequestValidatior authenticationValidator,
        AuthorizationRequestValidator authorizationValidator
        ) : ControllerBase
    {
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticationRequestModel request)
        {
            try
            {
                request.Password = await authorizationSecurityService.UnprotectAsync(request.Password!);
                var validationResult =
                    await authenticationValidator.ValidateAsync(request!);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new AuthenticationResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await authenticationNotificationService.SendAuthenticationMessageAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthenticationResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("authorize")]
        public async Task<IActionResult> AuthorizeAsync([FromBody] AuthorizationRequestModel request)
        {
            try
            {
                var validationResult =
                    await authorizationValidator.ValidateAsync(request!);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new AuthorizationResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await authorizationService.AuthorizeAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthorizationResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost("set-cookies")]
        public IActionResult SetCookies([FromBody] AuthorizationResponseModel data)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            Response.Cookies.Append(data.TokenName, data.TokenValue!, options);
            Response.Cookies.Append(data.UserIdName, data.UserIdValue!, options);
            Response.Cookies.Append(data.UserEmailAddressName, data.UserEmailAddressValue!, options);
            Response.Cookies.Append(data.AdminName, data.AdminValue!, options);
            return Ok();
        }

        [HttpPost("delete-cookies")]
        public IActionResult DeleteCookies()
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            var data = new AuthorizationResponseModel();

            Response.Cookies.Delete(data.TokenName, options);
            Response.Cookies.Delete(data.UserIdName, options);
            Response.Cookies.Delete(data.UserEmailAddressName, options);
            Response.Cookies.Delete(data.AdminName, options);

            // fallback (important)
            var expired = new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            Response.Cookies.Append(data.TokenName, "", expired);
            Response.Cookies.Append(data.UserIdName, "", expired);
            Response.Cookies.Append(data.UserEmailAddressName, "", expired);
            Response.Cookies.Append(data.AdminName, "", expired);

            return Ok();
        }

        
    }



}