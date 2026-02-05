using System.ComponentModel.DataAnnotations;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IValidationService
    {
        Task<bool> ValidateUserCredentialsAsync(string emailAddress, string password);
    }
    public class ValidationService(
        IConfiguration configuration,
        IEncryptionGatewayService encryptionGatewayService,
        IUserGatewayService userGatewayService 
        ) : ServiceBase(configuration), IValidationService
    {
        public async Task<bool> ValidateUserCredentialsAsync([Required] string emailAddress, [Required] string password)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email or password argument was not provided");
            }
            var user = await userGatewayService.GetUserAsync(emailAddress, true);

            if (user == null)
            {
                return await Task.FromResult(false);
            }

            if (!user.EmailAddressVerified)
            {
                return await Task.FromResult(false);
            }

            var unprotectedUserPassword = await encryptionGatewayService.UnprotectAsync(user.Password!, true);

            return await Task.FromResult(password == unprotectedUserPassword);
        }
    }
}

