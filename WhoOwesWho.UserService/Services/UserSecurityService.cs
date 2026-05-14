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
            if (value == string.Empty)
            {
                return value;
            }

            if (value is null)
            {
                throw new Exception("Security service has null value entered for protection");
            }

            if (value.IsValid() || value.IsGuid() || force)
            {
                return await encryptionGatewayService.ProtectAsync(value, true);
            }
            return value;
        }

        public async Task<string> UnprotectAsync(string value, bool force = false)
        {
            if (value == string.Empty)
            {
                return value;
            }
            if (value is null)
            {
                throw new Exception("Security service has null value entered for unprotection");
            }

            if (!value.IsValid() && !value.IsGuid() || force)
            {
                return await encryptionGatewayService.UnprotectAsync(value, true);
            }
            return value;
        }
    }
}
