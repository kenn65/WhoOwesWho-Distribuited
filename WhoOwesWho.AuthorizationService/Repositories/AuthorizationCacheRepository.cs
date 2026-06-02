using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Repositories
{

    public interface IAuthorizationCacheRepository
    {
        Task SaveUserAsync(UserMessageRequestModel user);
        Task SaveRefreshTokenAsync(RefreshTokenModel refreshTokenModel);
        Task<RefreshTokenModel?> GetRefreshTokenAsync(string id);
        Task<bool> DeleteRefreshTokenAsync(string id);
        Task<UserMessageResponseModel?> GetUserAsync(string emailAddress);
        Task<bool> GetUserExistAsync(string emailAddress);
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

        public async Task<bool> GetUserExistAsync(string emailAddress)
        {
            var value = await GetUserAsync(emailAddress);
            return value != null;
        }

        public async Task SaveUserAsync(UserMessageRequestModel user)
        {
            var json = JsonSerializer.Serialize(user);
            await db.StringSetAsync($"user:{user.EmailAddress}", json);
            await db.StringSetAsync($"user:{user.Id}", json);
        }

        public async Task SaveRefreshTokenAsync(RefreshTokenModel refreshTokenModel)
        {
            var json = JsonSerializer.Serialize(refreshTokenModel);
            await db.StringSetAsync($"refresh:{refreshTokenModel.Token}", json);
        }

        public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string id)
        {
            var value = await db.StringGetAsync($"refresh:{id}");
            return value.HasValue
                ? JsonSerializer.Deserialize<RefreshTokenModel>(value.ToString()!)
                : null;
        }

        public async Task<bool> DeleteRefreshTokenAsync(string id)
        {
            return await db.StringDeleteAsync($"refresh:{id}",
                    ValueCondition.Exists, CommandFlags.None);
        }
    }
}

