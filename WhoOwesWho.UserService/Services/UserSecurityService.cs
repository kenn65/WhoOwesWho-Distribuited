using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserSecurityService
    {
        public Task<string> ProtectAsync(string value);
        public Task<string> UnprotectAsync(string value);
    }


    public class UserSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IUserSecurityService
    {
        public async Task<string> ProtectAsync(string value)
        {
            return await encryptionGatewayService.ProtectAsync(value, true);
        }

        public async Task<string> UnprotectAsync(string value)
        {
            return await encryptionGatewayService.UnprotectAsync(value, true);
        }
    }
}
