using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services.Gateways
{
    public interface IEventGatewayService
    {
        Task<EventModel> GetUserEventAsync(string userId, string token, bool encode, bool active);
        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool encode, bool active);
    }
    public class EventGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IEventGatewayService
    {
        public async Task<EventModel> GetUserEventAsync(string userId, string token, bool encode, bool active)
        {
            return await Get<EventModel>($"{AppSettings.EventMicroServiceBaseAddress}/single/get/user",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                new Dictionary<string, dynamic>
                {
                    { "userId", userId },
                    { "active", active }
                },
                token);
        }

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool encode,
            bool active)
        {
            return await Get<IEnumerable<UserModel>>($"{AppSettings.EventMicroServiceBaseAddress}/assignment/users",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                new Dictionary<string, dynamic>
                {
                    { "eventId", eventId },
                    { "active", active }
                },
                token);
        }
    }
}
