using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Validators;

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
        IUserPublishingService userPublishingService,
        SignUpRequestValidatior signUpValidator,
        UpdateUserRequestValidator updateUserValidator,
        ForgotPasswordRequestValidator forgotPasswordValidator,
        ResetPasswordRequestValidator resetPasswordValidator,
        ChangePasswordRequestValidator changePasswordValidator
        ) : ControllerBase
    {
        [HttpPut]
        [Route("signup")]
        public async Task<IActionResult?> CreateUserAsync([FromBody] SignUpRequestModel request)
        {
            try
            {
                request?.Entity?.Password =
                    await userSecurityService.UnprotectAsync(
                        request.Entity.Password!);

                var validationResult =
                    await signUpValidator.ValidateAsync(request!);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new UserModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                request?.Entity?.Password  = await userSecurityService.ProtectAsync(request.Entity.Password!, true);
                return Ok(await userCreationService.CreateUserAsync(request!));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("{emailAddress}")]
        public async Task<IActionResult> GetUserByEmailAddressAsync(string emailAddress, [FromQuery] bool complete)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailAddress))
                {
                    return BadRequest(new UserModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.EmailArgumentError
                    });
                }
                return Ok(await userLookupService.GetSingleUserByEmailAddressAsync(emailAddress, complete));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("{id}/{complete}")]
        public async Task<IActionResult> GetUserById(string id, bool complete)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new UserModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                var user = await userLookupService.GetSingleUserByIdAsync(Guid.Parse(id), complete);

                if (user is null)
                {
                    return BadRequest(new UserModel
                    {
                        Message = Constants.GlobalErrorMessages.UnexpectedError
                    });
                }
                return Ok(user);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserModel
                {
                    Message = e.Message
                });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new UserModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPatch]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserAsync(Guid userId, [FromBody] UserUpdateRequestModel request)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new UserModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.UserIdArgumentError
                    });
                }
                                
                var validationResult =
                    await updateUserValidator.ValidateAsync(request);

                if (!validationResult.IsValid && !(await userValidationService.DoesFullNameExistAsync(userId!, request.FullName!)))
                {
                    return BadRequest(new UserModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }                
                request.Id = userId;
                return Ok(await userCommandService.UpdateUserAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("emailaddress/verify")]
        public async Task<IActionResult> VerifyEmailAddressAsync([FromBody] VerificationRequestModel request)
        {
            try
            {
                var response = await userValidationService.VerifyUserEmailAddressAsync(request.EmailAddress!);
                var entity = response.Adapt<UserMessageRequestModel>();
                if (entity is not null)
                {
                    await userPublishingService.SendUserAsync(entity!);
                }
                response!.Message = Constants.UserUpdatingErrorMessages.EmailVerificationSucceeded;
                return Ok(response);
            }
            catch 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new UserModel
                {
                    Message = Constants.GlobalErrorMessages.UnexpectedError
                });
            }
        }

        [HttpPost]
        [Route("password/forgot")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequestModel request)
        {
            try
            {
                var validationResult = await forgotPasswordValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ForgotPasswordResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                return Ok(await passwordRecoveryService.SendPasswordRecoveryEmailAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ForgotPasswordResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("password/reset/verify/{emailAddress}/{forgotPasswordToken}")]
        public async Task<IActionResult> VerifyResetPasswordAsync(string emailAddress, string forgotPasswordToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailAddress))
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.EmailArgumentError
                    });
                }

                if (string.IsNullOrWhiteSpace(forgotPasswordToken)) 
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.ForgotPasswordTokenArgumentError
                    });
                }
                
                emailAddress = await userSecurityService.UnprotectAsync(emailAddress);
                forgotPasswordToken = await userSecurityService.UnprotectAsync(forgotPasswordToken);
                
                return Ok(await resetPasswordService.VerifyResetPassword(emailAddress!, forgotPasswordToken!));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResetPasswordResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("password/reset")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequestModel request)
        {
            try
            {
                request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
                request.NewPassword = await userSecurityService.UnprotectAsync(request.NewPassword!);
                request.NewPasswordRepeat = await userSecurityService.UnprotectAsync(request.NewPasswordRepeat!);
                
                var validationResult = await resetPasswordValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }
                //request.NewPassword = await userSecurityService.ProtectAsync(request.NewPassword, true);
                //request.NewPasswordRepeat = await userSecurityService.ProtectAsync(request.NewPasswordRepeat, true);
                return Ok(await resetPasswordService.ResetPasswordAsync(request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResetPasswordResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("password/change")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequestModel request)
        {
            try
            {
                request.Password = await userSecurityService.UnprotectAsync(request.Password!);
                request.NewPassword1 = await userSecurityService.UnprotectAsync(request.NewPassword1!);
                request.NewPassword2= await userSecurityService.UnprotectAsync(request.NewPassword2!);

                var validationResult = await changePasswordValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ChangePasswordResponseModel
                    {
                        Message = validationResult.Errors.First().ErrorMessage
                    });
                }

                var response = await changePasswordService.ChangePasswordAsync(request);
                if (response!.Success)
                {
                    var user = await userLookupService.GetSingleUserByEmailAddressAsync(request.EmailAddress!, true);
                    var entity = user.Adapt<UserMessageRequestModel>();
                    await userPublishingService.SendUserAsync(entity!);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ChangePasswordResponseModel
                {
                    Message = e.Message
                });
            }
        }
    }
}
