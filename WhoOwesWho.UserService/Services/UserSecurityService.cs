using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserSecurityService
    {
        public Task<string> ProtectAsync(string value, bool force = false);
        public Task<string> UnprotectAsync(string value, bool force = false);
    }

    public class UserSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IUserSecurityService
    {
        public async Task<string> ProtectAsync(string value, bool force = false)
        {
            if (value is null)
            {
                throw new Exception("Invalid value entered");
            }

            if (value.IsValid() || value.IsGuid() || force)
            {
                return await encryptionGatewayService.ProtectAsync(value, true);
            }
            return value;
        }

        public async Task<string> UnprotectAsync(string value, bool force = false)
        {
            if (value is null)
            {
                throw new Exception("Invalid value entered");
            }

            if (!value.IsValid() && !value.IsGuid() || force)
            {
                return await encryptionGatewayService.UnprotectAsync(value, true);
            }
            return value;
        }
    }
}
