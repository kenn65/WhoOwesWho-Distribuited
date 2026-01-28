using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Services.Gateways
{
    public interface IEventGatewayService
    {
        Task<EventResponseModel> GetEventAsync(string eventId, string token, bool encode, bool active);
        Task<EventResponseModel> GetUserEventAsync(string userId, string token, bool encode, bool active);
        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool encode, bool active);
    }

    public class EventGatewayService(IConfiguration configuration)
        : GatewayServiceBase(configuration), IEventGatewayService
    {
        public async Task<EventResponseModel> GetEventAsync(string eventId, string token, bool encode, bool active)
        {
            try
            {
                return await Get<EventResponseModel>($"{AppSettings.EventMicroServiceBaseAddress}/single/get",
                    AppSettings.EventMicroServiceApiKey!,
                    encode,
                    new Dictionary<string, dynamic>
                    {
                        { "id", eventId },
                        { "active", active }
                    },
                    token);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<EventResponseModel> GetUserEventAsync(string userId, string token, bool encode, bool active = true)
        {
            return await Get<EventResponseModel>($"{AppSettings.EventMicroServiceBaseAddress}/single/get/user",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                new Dictionary<string, dynamic>
                {
                    { "userId", userId },
                    { "active", active }
                },
                token);
        }

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool encode, bool active)
        {
            return await Get<IEnumerable<UserModel>>($"{AppSettings.EventMicroServiceBaseAddress}/assignment/users",
                AppSettings.EventMicroServiceApiKey!,
                encode,
                new Dictionary<string, dynamic>
                {
                    { "eventId", eventId},
                    { "active", active }
                },
                token);
        }
    }
}
