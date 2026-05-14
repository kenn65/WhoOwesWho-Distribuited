using Mapster;
using WhoOwesWho.Shared.Auxiliaries;
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
        IUserSecurityService userSecurityService
        ) : ServiceBase(configuration), IPasswordRecoveryService
    {
        public async Task<ForgotPasswordResponseModel> SendPasswordRecoveryEmailAsync(ForgotPasswordRequestModel request)
        {
            var response = new ForgotPasswordResponseModel();
            try
            {
                var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress, false) ?? throw new Exception(Constants.PasswordRecoveryErrorMessages.UserNotFound);
                var entity = user.Adapt<UserMessageRequestModel>();
                var forgotPasswordToken = await userSecurityService.ProtectAsync(AppSettings.ForgotPasswordTokenSecret);

                await userNotificationService.SendPasswordRecoveryMessage(entity!, request.Host!, forgotPasswordToken);
                response.Success = true;
                response.Message = Constants.PasswordRecoveryErrorMessages.SuccessfullySent;
                return response;
            }
            catch (Exception)
            {
                return new ForgotPasswordResponseModel
                {
                    Message = Constants.GlobalErrorMessages.UnexpectedError
                };
            }
        }
    }
}

