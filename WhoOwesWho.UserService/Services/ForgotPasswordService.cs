using Microsoft.AspNetCore.Components;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;

namespace WhoOwesWho.UserService.Services
{
    public interface IForgotPasswordService
    {
        public Task<bool> SendForgotPasswordEmailAsync(ForgotPasswordRequestModel requestModel);
    }
    public class ForgotPasswordService(
        IConfiguration configuration, 
        IDataQueryService dataSelectionService, 
        IDataMutationService dataModificationService,
        IEncryptionGatewayService encryptionGatewayService,
        IMessagingPublisher messagingPublisher
       ) : ServiceBase(configuration), IForgotPasswordService
    {
        public async Task<bool> SendForgotPasswordEmailAsync(ForgotPasswordRequestModel request)
        {
            try
            {
                var user = await dataSelectionService.GetSingleUserByEmailAddressAsync(request.EmailAddress, false);
                if (user == null)
                {
                    return false; // User not found
                }

                var forgotPasswordToken = await encryptionGatewayService.ProtectAsync(AppSettings.ForgotPasswordTokenSecret, true);
                                                
                await SendForgotPasswordMessage(user!, request.Host!, forgotPasswordToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task SendForgotPasswordMessage(UserModel entity, string host, string forgotPasswordToken)
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


                await dataModificationService.DeleteForgotPasswordTokenAsync(entity.Id);
                if (!await dataModificationService.CreateForgotPasswordTokenAsync(new ForgotPasswordTokenModel
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
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }
        }
    }
}

