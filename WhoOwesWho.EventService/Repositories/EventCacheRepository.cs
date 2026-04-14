using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Repositories
{
    public interface IEventCacheRepository
    {
        Task<UserMessageResponseModel?> GetUserByIdAsync(string id);
        Task DeleteActiveEventAsync(string eventId);
    }
    public class EventCacheRepository(IDatabase db) : IEventCacheRepository
    {
        public async Task<UserMessageResponseModel?> GetUserByIdAsync(string id)
        {
            var value = await db.StringGetAsync($"user:{id}");
            return value.HasValue
                ? JsonSerializer.Deserialize<UserMessageResponseModel>(value!.ToString())
                : null;
        }

        public async Task DeleteActiveEventAsync(string eventId)
        {
            await db.KeyDeleteAsync($"activeevent:{eventId}");
        }
    }
}
