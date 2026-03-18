using Mapster;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IPasswordRecoveryService
    {   
        public Task<ForgotPasswordResponseModel> SendPasswordRecoveryEmailAsync(ForgotPasswordRequestModel requestModel);
    }
    public class PasswordRecoveryService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserNotificationService userNotificationService,
        IUserSecurityService userSecurityService,
        IUserValidationService userValidationService
       ) : ServiceBase(configuration), IPasswordRecoveryService
    {
        public async Task<ForgotPasswordResponseModel> SendPasswordRecoveryEmailAsync(ForgotPasswordRequestModel request)
        {
            var response = new ForgotPasswordResponseModel();
            try
            {
                request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
                if (!request.EmailAddress.IsValid())
                {
                    response.Message = "Invalid e-mail address provided.";
                    return response;
                }
                if (string.IsNullOrWhiteSpace(request.Host))
                {
                    response.Message = "Host is not provided.";
                    return response;
                }
                var checkEmailAddress = await userValidationService.ValidateEmailAsync(request.EmailAddress!, true);

                if (!checkEmailAddress.isValid)
                {
                    response.Message = checkEmailAddress.errorMessage;
                    return response;
                }
                var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress, false);
                if (user is null)
                {
                    response.Message = "Could not find user, please try again.";
                    return response;
                }

                var entity = user.Adapt<UserMessageRequestModel>();
                var forgotPasswordToken = await userSecurityService.ProtectAsync(AppSettings.ForgotPasswordTokenSecret);

                await userNotificationService.SendPasswordRecoveryMessage(entity!, request.Host!, forgotPasswordToken);
                response.Success = true;
                response.Message = "A password reset link sent to your e-mail address.";
                return response;
            }
            catch (Exception)
            {
                response.Message = "An unexpected error occurred, please try again.";
                return response;
            }
        }
    }
}

