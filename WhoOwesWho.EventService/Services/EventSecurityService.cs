using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;

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
            return await encryptionGatewayService.ProtectAsync(value);
        }

        public async Task<string> UnprotectAsync(string value)
        {
            return await encryptionGatewayService.UnprotectAsync(value);
        }
    }
}
