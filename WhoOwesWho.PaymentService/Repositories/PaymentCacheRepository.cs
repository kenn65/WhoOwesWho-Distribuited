
using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.PaymentService.Repositories
{
    public interface IPaymentCacheRepository
    {
        Task<UserMessageResponseModel?> GetUserByIdAsync(string id);
        Task<EventMessageResponseModel> GetEventByIdAsync(string id, bool active);
        Task SaveActiveEventAsync(EventMessageRequestModel eventMessageRequestModel);
        Task SaveInactiveEventAsync(EventMessageRequestModel eventMessageRequestModel);
       

    }
    public class PaymentCacheRepository(IDatabase db) : IPaymentCacheRepository
    {
        public async Task<EventMessageResponseModel> GetEventByIdAsync(string id, bool active)
        {
            if (active)
            {
                return await GetActiveEventByIdAsync(id) ?? throw new KeyNotFoundException($"Active event with id {id} not found.");
            }
            else
            {
                return await GetInactiveEventByIdAsync(id) ?? throw new KeyNotFoundException($"Inactive event with id {id} not found.");
            }
        }

        public async Task<UserMessageResponseModel?> GetUserByIdAsync(string id)
        {
            var value = await db.StringGetAsync($"user:{id}");
            return value.HasValue
                ? JsonSerializer.Deserialize<UserMessageResponseModel>(value!.ToString())
                : null;
        }

        public async Task SaveActiveEventAsync(EventMessageRequestModel eventMessageRequestModel)
        {
            var json = JsonSerializer.Serialize(eventMessageRequestModel);
            await db.StringSetAsync($"activeevent:{eventMessageRequestModel.Id}", json);
        }

        public async Task SaveInactiveEventAsync(EventMessageRequestModel eventMessageRequestModel)
        {
            var json = JsonSerializer.Serialize(eventMessageRequestModel);
            await db.StringSetAsync($"inactiveevent:{eventMessageRequestModel.Id}", json);
        }
        
        private async Task<EventMessageResponseModel?> GetActiveEventByIdAsync(string id)
        {
            var value = await db.StringGetAsync($"activeevent:{id}");
            var response = value.HasValue
                ? JsonSerializer.Deserialize<EventMessageResponseModel>(value!.ToString())
                : null;
            if (response is null)
            {
                return null;
            }
            if (!response!.Settled)
            {
                return response;
            }
            return null;
        }

        private async Task<EventMessageResponseModel?> GetInactiveEventByIdAsync(string id)
        {
            var value = await db.StringGetAsync($"inactiveevent:{id}");
            var response = value.HasValue
                ? JsonSerializer.Deserialize<EventMessageResponseModel>(value!.ToString())
                : null;
            if (response is null)
            {
                return null;
            }
            if (response!.Settled)
            {
                return response;
            }
            return null;
        }

        
    }
}
