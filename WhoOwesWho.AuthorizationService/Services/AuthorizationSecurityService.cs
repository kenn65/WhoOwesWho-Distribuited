using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationSecurityService
    {
        public Task<string> ProtectAsync(string value);
        public Task<string> UnprotectAsync(string value);

        Task<AuthorizationResponseModel> ProtectCookiesAsync(UserModel user, string token, bool encode);
    }

    public class AuthorizationSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IAuthorizationSecurityService
    {
        public async Task<string> ProtectAsync(string value)
        {
            return await encryptionGatewayService.ProtectAsync(value, true);
        }
                
        public async Task<string> UnprotectAsync(string value)
        {
            return await encryptionGatewayService.UnprotectAsync(value, true);
        }

        public async Task<AuthorizationResponseModel> ProtectCookiesAsync(UserModel user, string token, bool encode)
        {
            return await encryptionGatewayService.ProtectCookiesAsync(user, token, encode);
        }



    }
}
