
using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.UserService.Repositories
{
    public interface IUserCacheRepository
    {
        Task<UserMessageResponseModel?> GetUserByIdAsync(string id);
        Task<EventMessageResponseModel?> GetActiveEventByIdAsync(string id);
        
        public class UserCacheRepository(IDatabase db) : IUserCacheRepository
        {
            public async Task<UserMessageResponseModel?> GetUserByIdAsync(string id)
            {
                var value = await db.StringGetAsync($"user:{id}");
                return value.HasValue
                    ? JsonSerializer.Deserialize<UserMessageResponseModel>(value!)
                    : null;
            }

            public async Task<EventMessageResponseModel?> GetActiveEventByIdAsync(string id)
            {
                var value = await db.StringGetAsync($"activeevent:{id}");
                var response = value.HasValue
                    ? JsonSerializer.Deserialize<EventMessageResponseModel>(value!)
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

           
        }
    }
}
