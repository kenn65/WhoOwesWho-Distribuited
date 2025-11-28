using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.UserService.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;

namespace WhoOwesWho.UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        IDataMutationService dataModificationService,
        IDataQueryService dataSelectionService,
        IValidationService validationService,
        IForgotPasswordService forgotPasswordService,
        IResetPasswordService resetPasswordService,
        IChangePasswordService changePasswordService,
        IUserService userService,
        IUnprotectValueMessageSender unprotectValueEventService) : ControllerBase
    {
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult?> Create([FromBody][Required] SignUpRequestModel request)
        {
            var actionResult = new SignUpResponseModel();
            try
            {

                request.Entity!.EmailAddress = await unprotectValueEventService.SendAsync(request.Entity!.EmailAddress!);
                request.Entity!.Password = await unprotectValueEventService.SendAsync(request.Entity!.Password!);
              
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

                var passwordCheck = await validationService.ValidatePasswordAsync(request.Entity.Password!);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = passwordCheck.errorMessage;
                    return Ok(actionResult);
                }

                var check = await dataModificationService.CreateUserAsync(request.Entity, request.Host!) is not null;
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
        [Route("{emailAddress}")]
        public async Task<IActionResult> GetByEmailAddress(string emailAddress)
        {
            try
            {
                var unprotectedValue = await unprotectValueEventService.SendAsync(emailAddress);
                return Ok(await dataSelectionService.GetSingleUserByEmailAddressAsync(unprotectedValue, false));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("single/email")]
        public async Task<IActionResult> GetUserByEmailAddress([Required] string emailAddress, [Required] bool complete)
        {
            try
            {
                var unprotectedValue = await unprotectValueEventService.SendAsync(emailAddress);
                return Ok(await dataSelectionService.GetSingleUserByEmailAddressAsync(unprotectedValue, complete));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



        [HttpGet]
        [Authorize]
        [Route("{idOrEmailAddress}/{complete}")]
        public async Task<IActionResult> Obsolete([Required] string idOrEmailAddress, [Required] bool complete)
        {
            try
            {
                var unprotectedValue = await unprotectValueEventService.SendAsync(idOrEmailAddress);
                var checkEmail = await validationService.ValidateEmailAsync(unprotectedValue);

                return checkEmail.isValid
                    ? Ok(await dataSelectionService.GetSingleUserByEmailAddressAsync(unprotectedValue, complete))
                    : Ok(await dataSelectionService.GetSingleUserByIdAsync(Guid.Parse(unprotectedValue), complete));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("single")]
        public async Task<IActionResult> GetByIdOrEmailAddress([Required] string idOrEmailAddress, [Required] bool complete)
        {
            try
            {
                var unprotectedValue = await unprotectValueEventService.SendAsync(idOrEmailAddress);
                var checkEmail = (await validationService.ValidateEmailAsync(unprotectedValue, true));

                var user = checkEmail.isValid
                        ? await dataSelectionService.GetSingleUserByEmailAddressAsync(unprotectedValue, complete)
                        : await dataSelectionService.GetSingleUserByIdAsync(Guid.Parse(unprotectedValue), complete);

                if (user == null)
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await dataSelectionService.GetAllUsersAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UserModel? entity)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await userService.UpdateUserAsync(entity!, token));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("emailaddress/verify")]
        public async Task<IActionResult> VerifyEmailAddress([Required][FromBody] VerificationRequestModel request)
        {
            try
            {
                request.EmailAddress = await unprotectValueEventService.SendAsync(request.EmailAddress!);
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
                request.EmailAddress = await unprotectValueEventService.SendAsync(request.EmailAddress!);
                
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

                var checkEmailDispatch = await forgotPasswordService.SendForgotPasswordEmailAsync(request);
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
        [Route("password/reset/verify")]
        public async Task<IActionResult> VerifyResetPassword([Required] string emailAddress, [Required] string forgotPasswordToken)
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
                request.EmailAddress = await unprotectValueEventService.SendAsync(request.EmailAddress!);
                request.NewPassword = await unprotectValueEventService.SendAsync(request.NewPassword!);
                request.NewPasswordRepeat = await unprotectValueEventService.SendAsync(request.NewPasswordRepeat!);
                
                if (request.NewPassword != request.NewPasswordRepeat)
                {
                    actionResult.Message = "The passwords does not match!";
                    return Ok(actionResult);
                }

                var user = await dataSelectionService.GetSingleUserByEmailAddressAsync(request.EmailAddress, true);
                if (user == null)
                {
                    actionResult.Message = $"Could not find the account with e-mail address: {request.EmailAddress}";
                    return Ok(actionResult);
                }

                var unprotectedUserPassword = await unprotectValueEventService.SendAsync(user.Password!);
                                
                if (unprotectedUserPassword == request.NewPassword)
                {
                    actionResult.Message = "The new password cannot be the same as the existing password.";
                    return Ok(actionResult);
                }

                var passwordCheck = await validationService.ValidatePasswordAsync(request.NewPassword!);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                passwordCheck = await validationService.ValidatePasswordAsync(request.NewPasswordRepeat!);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password repeated:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                var response = await resetPasswordService.ResetPasswordAsync(request);
                actionResult.Success = response!.Success;
                actionResult.Message = response.Message;
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
                request.EmailAddress = await unprotectValueEventService.SendAsync(request.EmailAddress!);
                request.Password = await unprotectValueEventService.SendAsync(request.Password!);
                request.NewPassword1 = await unprotectValueEventService.SendAsync(request.NewPassword1!);
                request.NewPassword2 = await unprotectValueEventService.SendAsync(request.NewPassword2!);
                
                var emailCheck = await validationService.ValidateEmailAsync(request.EmailAddress!, true);
                if (!emailCheck.isValid)
                {
                    actionResult.Message = emailCheck.errorMessage;
                    return Ok(actionResult);
                }

                var passwordCheck = await validationService.ValidatePasswordAsync(request.Password!);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For existing password:</strong><br />{passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }
                if (request.NewPassword1 != request.NewPassword2)
                {
                    actionResult.Message = "The passwords does not match!";
                    return Ok(actionResult);
                }

                passwordCheck = await validationService.ValidatePasswordAsync(request.NewPassword1!);
                if (!passwordCheck.isValid)
                {
                    actionResult.Message = $"<strong>For new password:</strong><br /> {passwordCheck.errorMessage}";
                    return Ok(actionResult);
                }

                passwordCheck = await validationService.ValidatePasswordAsync(request.NewPassword2!);
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
