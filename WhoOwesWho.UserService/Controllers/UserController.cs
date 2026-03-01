using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        IUserValidationService validationService,
        IPasswordRecoveryService passwordRecoveryService,
        IResetPasswordService resetPasswordService,
        IChangePasswordService changePasswordService,
        IUserSecurityService userSecurityService,
        IUserCommandService userCommandService,
        IUserLookupService userLookupService
        ) : ControllerBase
    {
        [HttpPut]
        [Route("signup")]
        public async Task<IActionResult?> CreateUserAsync([FromBody] SignUpRequestModel request)
        {
            var actionResult = new SignUpResponseModel();
            try
            {
                request.Entity!.EmailAddress = await userSecurityService.UnprotectAsync(request.Entity.EmailAddress!);
                
                if (string.IsNullOrWhiteSpace(request.Entity?.FullName))
                {
                    actionResult.Message = "Full name is required.";
                    return Ok(actionResult);
                }

                var emailCheck = await validationService.ValidateEmailAsync(request.Entity.EmailAddress!);
                if (!emailCheck.isValid)
                {
                    actionResult.Message = emailCheck.errorMessage;
                    return Ok(actionResult);
                }

                var passwordCheck = await validationService.ValidatePasswordAsync(request.Entity.Password!, true);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = passwordCheck.errorMessage;
                    return Ok(actionResult);
                }

                var check = await userCommandService.CreateUserAsync(request.Entity, request.Host!) is not null;
                actionResult.Success = check;
                actionResult.Message = !check
                    ? "An unexpected error occurred, please try again."
                    : "<p><strong>Sign up successful!</strong><br /> An e-mail has been sent to you for your account verification.</p>";
                return Ok(actionResult);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("{idOrEmailAddress}")]
        public async Task<IActionResult> GetUnautorizedUserByEmailAddressAsync(string idOrEmailAddress, [FromQuery] bool complete)
        {
            try
            {
                var unprotectedValue = await userSecurityService.UnprotectAsync(idOrEmailAddress);
                return Ok(await userLookupService.GetSingleUserByEmailAddressAsync(unprotectedValue, complete));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("{idOrEmailAddress}/{complete}")]
        public async Task<IActionResult> GetAuthorizedUserByIdOrEmailAddressAsync(string idOrEmailAddress, bool complete)
        {
            try
            {
                var unprotectedValue = await userSecurityService.UnprotectAsync(idOrEmailAddress);
                var checkEmail = await validationService.ValidateEmailAsync(unprotectedValue, true);

                var user = checkEmail.isValid
                    ? await userLookupService.GetSingleUserByEmailAddressAsync(unprotectedValue, complete)
                    : await userLookupService.GetSingleUserByIdAsync(Guid.Parse(unprotectedValue), complete);

                if (user is null)
                {
                    return Ok(new UserModel
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
        public async Task<IActionResult> Update(string userId, [FromBody] UserModel? entity)
        {
            try
            {
                var unprotectedUserId = await userSecurityService.UnprotectAsync(userId);    
                var token = HttpContext.ToTokenValue();
                entity!.Id = Guid.Parse(unprotectedUserId);
                return Ok(await userCommandService.UpdateUserAsync(entity!, token));
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

                return Ok(await validationService.VerifyUserEmailAddress(request.EmailAddress!));
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
            var actionResult = new ForgotPasswordResponseModel();

            try
            {
                request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);

                if (string.IsNullOrWhiteSpace(request.Host))
                {
                    actionResult.Message = "Host is not provided.";
                    return Ok(actionResult);
                }

                var checkEmailAddress = await validationService.ValidateEmailAsync(request.EmailAddress!, true);

                if (!checkEmailAddress.isValid)
                {
                    actionResult.Message = checkEmailAddress.errorMessage;
                    return Ok(actionResult);
                }

                var checkEmailDispatch = await passwordRecoveryService.SendPasswordRecoveryEmailAsync(request);
                actionResult.Success = checkEmailDispatch;
                actionResult.Message = !checkEmailDispatch
                    ? "An unexpected error occurred, please try again."
                    : "A password reset link sent to your e-mail address.";
                return Ok(actionResult);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Route("password/reset/verify/{emailAddress}/{forgotPasswordToken}")]
        public async Task<IActionResult> VerifyResetPassword(string emailAddress, string forgotPasswordToken)
        {
            try
            {
                if (emailAddress != "undefined" && forgotPasswordToken != "undefined")
                {
                    return Ok(await resetPasswordService.VerifyResetPassword(emailAddress, forgotPasswordToken));
                }
                return Ok(await Task.FromResult(new ResetPasswordResponseModel
                {
                    Message = "",
                    Success = true
                }));
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
            var actionResult = new ResetPasswordResponseModel();
            try
            {
                var emailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);

                var newPassword = await userSecurityService.UnprotectAsync(request.NewPassword!);

                var newPasswordRepeat = await userSecurityService.UnprotectAsync(request.NewPasswordRepeat!);

                if (newPassword != newPasswordRepeat)
                {
                    actionResult.Message = "The passwords does not match!";
                    return Ok(actionResult);
                }

                var user = await userLookupService.GetSingleUserByEmailAddressAsync(emailAddress, true);
                if (user is null)
                {
                    actionResult.Message = $"Could not find the account with e-mail address: {request.EmailAddress}";
                    return Ok(actionResult);
                }

                var unprotectedUserPassword = await userSecurityService.UnprotectAsync(user.Password!);

                if (unprotectedUserPassword == request.NewPassword)
                {
                    actionResult.Message = "The new password cannot be the same as the existing password.";
                    return Ok(actionResult);
                }

                var passwordCheck = await validationService.ValidatePasswordAsync(newPassword);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                passwordCheck = await validationService.ValidatePasswordAsync(newPassword);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password repeated:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                request.EmailAddress = emailAddress;
                var result = await resetPasswordService.ResetPasswordAsync(request);
                actionResult.Success = result!.Success;
                actionResult.Message = result.Message;
                return Ok(actionResult);
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
            var actionResult = new ResetPasswordResponseModel();
            try
            {
                request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
                var password = await userSecurityService.UnprotectAsync(request.Password!);
                var newPassword1 = await userSecurityService.UnprotectAsync(request.NewPassword1!);
                var newPassword2 = await userSecurityService.UnprotectAsync(request.NewPassword2!);

                var emailCheck = await validationService.ValidateEmailAsync(request.EmailAddress!, true);
                if (!emailCheck.isValid)
                {
                    actionResult.Message = emailCheck.errorMessage;
                    return Ok(actionResult);
                }

                var passwordCheck = await validationService.ValidatePasswordAsync(password);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For existing password:</strong><br />{passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }
                if (newPassword1 != newPassword2)
                {
                    actionResult.Message = "The passwords does not match!";
                    return Ok(actionResult);
                }

                passwordCheck = await validationService.ValidatePasswordAsync(newPassword1);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                passwordCheck = await validationService.ValidatePasswordAsync(newPassword2!);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password repeated:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                var response = await changePasswordService.ChangePasswordAsync(request);
                actionResult.Success = response!.Success;
                actionResult.Message = !response.Success
                    ? response.Message
                    : "Your password change completed successfully.";
                return Ok(actionResult);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
