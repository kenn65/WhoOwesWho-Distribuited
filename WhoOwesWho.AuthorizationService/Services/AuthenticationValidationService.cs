using System.ComponentModel.DataAnnotations;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Shared.Extensions;

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
                throw new ArgumentException("Email and/or password was not provided");
            }
            emailAddress = await authorizationSecurityService.UnprotectAsync(emailAddress);
            if (!emailAddress.IsValid())
            {
                return false;
            }

            if (!password.IsValid(AppSettings.PasswordLengthRequired, AppSettings.PasswordUppercaseRequired, AppSettings.PasswordDigitsRequired))
            {
                return false;
            }
            var user = await authorizationCacheRepository.GetUserAsync(emailAddress);

            if (user is null)
            {
                return false;
            }

            if (!user.EmailAddressVerified)
            {
                return false;
            }

            return password == user.Password;
        }
    }
}

