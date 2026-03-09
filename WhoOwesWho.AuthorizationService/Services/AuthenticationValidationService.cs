using System.ComponentModel.DataAnnotations;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthenticationValidationService
    {
        Task<bool> ValidateUserCredentialsAsync(string emailAddress, string password);
    }
    public class AuthenticationValidationService(
        IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository,
        IAuthorizationSecurityService authorizationSecurityService
        ) : ServiceBase(configuration), IAuthenticationValidationService
    {
        public async Task<bool> ValidateUserCredentialsAsync([Required] string emailAddress, [Required] string password)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email or password argument was not provided");
            }
            var unprotectedEmailAddress = await authorizationSecurityService.UnprotectAsync(emailAddress);
            var user = await authorizationCacheRepository.GetUserAsync(unprotectedEmailAddress);

            if (user is null)
            {
                return await Task.FromResult(false);
            }

            if (!user.EmailAddressVerified)
            {
                return await Task.FromResult(false);
            }
            
            return await Task.FromResult(password == user.Password);
        }
    }
}

