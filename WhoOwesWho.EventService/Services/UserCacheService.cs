using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IUserCacheService
    {
        Task<UserMessageResponseModel?> GetUserAsync(string id);
    }

    public class UserCacheService(IEventCacheRepository cache) : IUserCacheService
    {
        public async Task<UserMessageResponseModel?> GetUserAsync(string id)
        {
            return await cache.GetUserByIdAsync(id);
        }
    }
}
