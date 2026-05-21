using WhoOwesWho.AuthorizationService.Repositories;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthValidationService
    {
        Task<bool> DoesEmailExist(string emailAddress);
        Task<bool> IsPasswordValid(string emailAddress, string password);
    }
    public class AuthValidationService(
        IAuthorizationCacheRepository authorizationCacheRepository,
        IAuthorizationSecurityService authorizationSecurityService
        ) : IAuthValidationService
    {
        public async Task<bool> DoesEmailExist(string emailAddress)
        {
            return await authorizationCacheRepository.GetUserExistAsync(emailAddress);
        }

        public async Task<bool> IsPasswordValid(string emailAddress, string password) 
        {
            var response = await authorizationCacheRepository.GetUserAsync(emailAddress);
            var unprotectedPass = await authorizationSecurityService.UnprotectAsync(response?.Password!);
            return unprotectedPass == password;
        }
    }
}

