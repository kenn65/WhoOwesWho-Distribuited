using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventSecurityService
    {
        public Task<string> ProtectAsync(string value);
        public Task<string> UnprotectAsync(string value);
    }

    public class EventSecurityService(
        IConfiguration configuration, 
        IEncryptionGatewayService encryptionGatewayService
        ) : ServiceBase(configuration), IEventSecurityService
    {
        public async Task<string> ProtectAsync(string value)
        {
            if (value.IsValid() || value.IsGuid())
            {
                return await encryptionGatewayService.ProtectAsync(value);
            }
            return value;
        }

        public async Task<string> UnprotectAsync(string value)
        {
            if (!value.IsValid() && !value.IsGuid())
            {
                return await encryptionGatewayService.UnprotectAsync(value);
            }
            return value;
        }
    }
}
