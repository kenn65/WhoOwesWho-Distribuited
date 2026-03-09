using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Resolvers
{
    public interface IUserResolverService
    {
        Task<bool> CreateUserAsync(UserMessageRequestModel request);
    }

    public class UserResolverService(
        IAuthorizationSecurityService authorizationSecurityService, 
        IAuthorizationCacheRepository authorizationCacheRepository
        ) : IUserResolverService
    {
        public async Task<bool> CreateUserAsync(UserMessageRequestModel request)
        {
            try
            {
                if (!await authorizationSecurityService.ValidateApiKey(request.ApiKey))
                {
                    if (!await authorizationSecurityService.ValidateApiKey(request.ApiKey))
                    {
                        throw new UnauthorizedAccessException("Invalid API Key");
                    }
                }
                await authorizationCacheRepository.SaveUserAsync(request);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }
    }
}
