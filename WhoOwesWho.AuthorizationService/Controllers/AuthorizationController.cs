using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using WhoOwesWho.AuthorizationService.Repositories;
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
        IAuthorizationCacheRepository authorizationCacheRepository
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
                var refreshToken = request.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return Unauthorized(new AuthorizationResponseModel
                    {
                        Success = false,
                        Message = "Refresh token missing"
                    });
                }

                var existingRefreshToken = await authorizationCacheRepository.GetRefreshTokenAsync(refreshToken);

                if (existingRefreshToken is null)
                {
                    return Unauthorized(
                        new AuthorizationResponseModel
                        {
                            Success = false,
                            Message = "Invalid refresh token"
                        });
                }

                if (existingRefreshToken.ExpiresUtc < DateTime.UtcNow)
                {
                    return Unauthorized(new AuthorizationResponseModel
                    {
                        Success = false,
                        Message = "Refresh token expired"
                    });
                }

                var user = await authorizationCacheRepository.GetUserByIdAsync(existingRefreshToken.UserId.ToString());

                if (user is null)
                {
                    return Unauthorized(
                        new AuthorizationResponseModel
                        {
                            Success = false,
                            Message = "User not found"
                        });
                }

                //await authorizationCacheRepository.DeleteRefreshTokenAsync(refreshToken);

                var authorizationResponse = await authorizationService.AuthorizeAsync(
                            new AuthorizationRequestModel
                            {
                                EmailAddress = user.EmailAddress
                            });

                if (authorizationResponse is null || !authorizationResponse.Success)
                {
                    return Unauthorized(new AuthorizationResponseModel
                    {
                        Success = false,
                        Message = "Failed to generate JWT"
                    });
                }

                var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                var refreshModel =
                    new RefreshTokenModel
                    {
                        UserId = user.Id,
                        Token = newRefreshToken,
                        CreatedUtc = DateTime.UtcNow,
                        ExpiresUtc = DateTime.UtcNow.AddDays(90)
                    };
                var options = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                };
                await authorizationCacheRepository.SaveRefreshTokenAsync(refreshModel);
                authorizationResponse.RefreshValue = newRefreshToken;
                authorizationResponse.Success = true;
                return Ok(authorizationResponse);
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
                await authorizationCacheRepository.DeleteRefreshTokenAsync(request.RefreshToken);
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