using Mapster;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IPasswordRecoveryService
    {
        public Task<bool> SendPasswordRecoveryEmailAsync(ForgotPasswordRequestModel requestModel);
    }
    public class PasswordRecoveryService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserNotificationService userNotificationService,
        IUserSecurityService userSecurityService
       ) : ServiceBase(configuration), IPasswordRecoveryService
    {
        public async Task<bool> SendPasswordRecoveryEmailAsync(ForgotPasswordRequestModel request)
        {
            try
            {
                var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress, false);
                if (user is null)
                {
                    return false; // User not found
                }

                var entity = user.Adapt<UserMessageRequestModel>();
                var forgotPasswordToken = await userSecurityService.ProtectAsync(AppSettings.ForgotPasswordTokenSecret);

                await userNotificationService.SendPasswordRecoveryMessage(entity!, request.Host!, forgotPasswordToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

