using System.ComponentModel.DataAnnotations;
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
        IAuthorizationSecurityService authorizationSecurityService,
        IUserGatewayService userGatewayService 
        ) : ServiceBase(configuration), IAuthenticationValidationService
    {
        public async Task<bool> ValidateUserCredentialsAsync([Required] string emailAddress, [Required] string password)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email or password argument was not provided");
            }
            var user = await userGatewayService.GetUserAsync(emailAddress, true);

            if (user is null)
            {
                return await Task.FromResult(false);
            }

            if (!user.EmailAddressVerified)
            {
                return await Task.FromResult(false);
            }

            var unprotectedUserPassword = await authorizationSecurityService.UnprotectAsync(user.Password!);

            return await Task.FromResult(password == unprotectedUserPassword);
        }
    }
}

