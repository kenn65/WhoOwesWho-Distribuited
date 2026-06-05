using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Validators;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

namespace WhoOwesWho.AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(
        Services.IAuthorizationService authorizationService,
        IAuthenticationNotificationService authenticationNotificationService,
        IAuthorizationSecurityService authorizationSecurityService,
        AuthenticationRequestValidatior authenticationValidator,
        AuthorizationRequestValidator authorizationValidator,
        IRefreshTokenService refreshTokenService
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

        [HttpPost]
        [Route("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshAsync(RefreshRequestModel request)
        {
            try
            {
                var response = await refreshTokenService.RefreshTokenAsync(request);
                if (response is null || !response.Success)
                {
                    return Unauthorized(response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,

                    new AuthorizationResponseModel
                    {
                        Success = false,
                        Message = e.Message
                    });
            }
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> DeleteRefreshTokenFromCache(RefreshRequestModel request)
        {
            try
            {
                await refreshTokenService.DeleteRefreshTokenAsync(request.RefreshToken);
                return Ok(new CookiesDeletionResponseModel
                {
                    Success = true
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,

                    new CookiesDeletionResponseModel
                    {
                        Success = false,
                        Message = e.Message
                    });
            }
        }
    }
}