using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Services.Gateways
{
    public interface IEventGatewayService
    {
        Task<EventModel> GetEventAsync(string eventId, string token, bool encode, bool active);
        Task<EventModel> GetUserEventAsync(string userId, string token, bool encode, bool active);
        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool encode, bool active);
    }

    public class EventGatewayService(IConfiguration configuration)
        : GatewayServiceBase(configuration), IEventGatewayService
    {
        public async Task<EventModel> GetEventAsync(string eventId, string token, bool encode, bool active)
        {
            try
            {
                return await Get<EventModel>($"{AppSettings.EventMicroServiceEventsBaseAddress}/{eventId}/{active}",
                    AppSettings.EventMicroServiceApiKey!,
                    encode,
                    parameters: null,
                    token);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<EventModel> GetUserEventAsync(string userId, string token, bool encode, bool active = true)
        {
            return await Get<EventModel>($"{AppSettings.EventMicroServiceUserEventsBaseAddress}/{userId}/{active}",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                parameters: null,
                token);
        }

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool encode, bool active)
        {
            return await Get<IEnumerable<UserModel>>($"{AppSettings.EventMicroServiceEventUsersBaseAddress}/{eventId}/{active}",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                parameters: null,
                token);
        }
    }
}
