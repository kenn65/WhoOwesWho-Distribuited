using WhoOwesWho.AuthorizationService.Repositories;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthValidationService
    {
        Task<bool> DoesEmailExist(string emailAddress);
    }
    public class AuthValidationService(
        IAuthorizationCacheRepository authorizationCacheRepository
        ) : IAuthValidationService
    {
        public async Task<bool> DoesEmailExist(string emailAddress)
        {
            return await authorizationCacheRepository.GetUserExistAsync(emailAddress);
        }
    }
}

