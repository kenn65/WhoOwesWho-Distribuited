using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services.Gateways
{
    public interface IEventGatewayService
    {
        Task<EventResponseModel> GetUserEventAsync(string userId, string token, bool encode, bool active);
        Task<IEnumerable<UserMessageRequestModel>> GetEventUsersAsync(string eventId, string token, bool encode, bool active);
    }
    public class EventGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IEventGatewayService
    {
        public async Task<EventResponseModel> GetUserEventAsync(string userId, string token, bool encode, bool active)
        {
            return await Get<EventResponseModel>($"{AppSettings.EventMicroServiceUserEventsBaseAddress}/{userId}/{active}",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                parameters: null,
                token);
        }

        public async Task<IEnumerable<UserMessageRequestModel>> GetEventUsersAsync(string eventId, string token, bool encode,
            bool active)
        {
            return await Get<IEnumerable<UserMessageRequestModel>>($"{AppSettings.EventMicroServiceEventUsersBaseAddress}/{eventId}/{active}",
                 AppSettings.EventMicroServiceApiKey!,
                 encode,
                 parameters: null,
                 token);
        }
    }
}
