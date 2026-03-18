using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.Shared.Models;
using Mapster;
using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        IUserCreationService userCreationService,
        IUserValidationService userValidationService,
        IPasswordRecoveryService passwordRecoveryService,
        IResetPasswordService resetPasswordService,
        IChangePasswordService changePasswordService,
        IUserSecurityService userSecurityService,
        IUserCommandService userCommandService,
        IUserLookupService userLookupService,
        IUserPublishingServicee userPublishingService
        ) : ControllerBase
    {
        [HttpPut]
        [Route("signup")]
        public async Task<IActionResult?> CreateUserAsync([FromBody] SignUpRequestModel request)
        {
            try
            {
                return Ok(await userCreationService.CreateUserAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet]
        [Route("{emailAddress}")]
        public async Task<IActionResult> GetUnautorizedUserByEmailAddressAsync(string emailAddress, [FromQuery] bool complete)
        {
            try
            {
                emailAddress = await userSecurityService.UnprotectAsync(emailAddress);
                return Ok(await userLookupService.GetSingleUserByEmailAddressAsync(emailAddress, complete));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("{id}/{complete}")]
        public async Task<IActionResult> GetAuthorizedUserByIdAddressAsync(string id, bool complete)
        {
            try
            {
                id = await userSecurityService.UnprotectAsync(id);
                var user = await userLookupService.GetSingleUserByIdAsync(Guid.Parse(id), complete);

                if (user is null)
                {
                    return Ok(new UserMessageResponseModel
                    {
                        Message = "An unexpected error occurred. Please, try again."
                    });
                }
                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsersAsync()
        {
            try
            {
                return Ok(await userLookupService.GetAllUsersAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> Update(string userId, [FromBody] UserUpdateRequestModel? entity)
        {
            try
            {
                userId = await userSecurityService.UnprotectAsync(userId);
                entity!.Id = Guid.Parse(userId);
                return Ok(await userCommandService.UpdateUserAsync(entity!));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("emailaddress/verify")]
        public async Task<IActionResult> VerifyEmailAddress([FromBody] VerificationRequestModel request)
        {
            try
            {
                request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
                var response = await userValidationService.VerifyUserEmailAddress(request.EmailAddress!);
                if (response!.Success)
                {
                    var entity = response.Adapt<UserMessageRequestModel>();
                    await userPublishingService.SendUserAsync(entity);
                }
                return Ok(response);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("password/forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel request)
        {
            try
            {
                request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
                return Ok(await passwordRecoveryService.SendPasswordRecoveryEmailAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("password/reset/verify/{emailAddress}/{forgotPasswordToken}")]
        public async Task<IActionResult> VerifyResetPassword([Required] string emailAddress, [Required] string forgotPasswordToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(forgotPasswordToken))
                {
                    return Ok(new ResetPasswordResponseModel
                    {
                        Success = false,
                        Message = "emailAddress or forgotPasswordToken was not provided. Please, try again."
                    });
                }
                return Ok(await resetPasswordService.VerifyResetPassword(emailAddress!, forgotPasswordToken!));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        [Route("password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel request)
        {
            try
            {
                return Ok(await resetPasswordService.ResetPasswordAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("password/change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel request)
        {
            try
            {
                var response = await changePasswordService.ChangePasswordAsync(request);
                if (response!.Success)
                {
                    var user = await userLookupService.GetSingleUserByEmailAddressAsync(request.EmailAddress!, true);
                    var entity = user.Adapt<UserMessageRequestModel>();
                    await userPublishingService.SendUserAsync(entity);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
