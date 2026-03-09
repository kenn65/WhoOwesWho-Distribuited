using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserNotificationService
    {
        Task SendAccountConfirmationMessage(UserMessageRequestModel entity, string host);
        Task SendPasswordRecoveryMessage(UserMessageRequestModel entity, string host, string forgotPasswordToken);
    }


    public class UserNotificationService(
         IConfiguration configuration,
         IUserMutationRepository userMutationRepository,
         IMessagingPublisher messagingPublisher
         ) : ServiceBase(configuration), IUserNotificationService
    {
        public async Task SendAccountConfirmationMessage(UserMessageRequestModel entity, string host)
        {
            try
            {
                var request = new MessagingRequestModel
                {
                    ApiKey = AppSettings.MessagingMicroServiceApiKey,
                    Host = host,
                    Type = "SignUp",
                    User = entity
                };

                await messagingPublisher.DispatchAsync(request);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }
        }

        public async Task SendPasswordRecoveryMessage(UserMessageRequestModel entity, string host, string forgotPasswordToken)
        {
            try
            {
                var request = new MessagingRequestModel
                {
                    ApiKey = AppSettings.MessagingMicroServiceApiKey,
                    Host = host,
                    Type = "ResetPassword",
                    User = entity,
                    ForgotPasswordToken = forgotPasswordToken
                };


                await userMutationRepository.DeleteForgotPasswordTokenAsync(entity.Id);
                if (!await userMutationRepository.CreateForgotPasswordTokenAsync(new ForgotPasswordTokenModel
                {
                    UserId = request.User.Id,
                    ForgotPasswordToken = request.ForgotPasswordToken,
                    ExpirationTime = DateTime.Now
                            .AddMinutes(int.Parse(AppSettings.ForgotPasswordExpirationTimeInMinutes)).Ticks
                }))
                {
                    throw new Exception("Failed to create forgot password token in the database.");
                }

                await messagingPublisher.DispatchAsync(request);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending forgot password message: {e.Message}",
                    e);
            }
        }
    }
}
