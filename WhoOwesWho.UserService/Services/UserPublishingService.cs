using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserPublishingService
    {
        Task SendUserAsync(UserMessageRequestModel user);
    }
    public class UserPublishingService(IConfiguration configuration, IUserPublisher userPublisher) : ServiceBase(configuration), IUserPublishingService
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
                throw new Exception($"{Constants.UserCreationErrorMessages.DispatchUserException} {e.Message}",
                    e);
            }
        }
    }
}
