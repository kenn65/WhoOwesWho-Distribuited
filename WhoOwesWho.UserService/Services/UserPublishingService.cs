using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserPublishingServicee
    {
        Task SendUserAsync(UserMessageRequestModel user);
    }
    public class UserPublishingService(IConfiguration configuration, IUserPublisher userPublisher) : ServiceBase(configuration), IUserPublishingServicee
    {
        public async Task SendUserAsync(UserMessageRequestModel user)
        {
            try
            {
                user.ApiKey = AppSettings.AuthorizationMicroServiceApiKey;
                await userPublisher.DispatchAsync(user);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }
        }
    }
}
