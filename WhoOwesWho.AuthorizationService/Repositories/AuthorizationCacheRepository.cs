using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Repositories
{

    public interface IAuthorizationCacheRepository
    {
        Task SaveUserAsync(UserMessageRequestModel user);
        Task<UserMessageResponseModel?> GetUserAsync(string emailAddress);
        Task<UserMessageResponseModel?> GetUserByIdAsync(string id);
    }

    public class AuthorizationCacheRepository(IDatabase db) : IAuthorizationCacheRepository
    {
        public async Task<UserMessageResponseModel?> GetUserAsync(string emailAddress)
        {
            var value = await db.StringGetAsync($"user:{emailAddress}");
            return value.HasValue
                ? JsonSerializer.Deserialize<UserMessageResponseModel>(value.ToString()!)
                : null;
        }

        public async Task<UserMessageResponseModel?> GetUserByIdAsync(string id)
        {
            var value = await db.StringGetAsync($"user:{id}");
            return value.HasValue
                ? JsonSerializer.Deserialize<UserMessageResponseModel>(value.ToString()!)
                : null;
        }

        public async Task SaveUserAsync(UserMessageRequestModel user)
        {
            var json = JsonSerializer.Serialize(user);
            await db.StringSetAsync($"user:{user.EmailAddress}", json);
            await db.StringSetAsync($"user:{user.Id}", json);
        }
    }


}
