using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services.Gateways
{
    public interface IEncryptionGatewayService
    {
        Task<string> ProtectAsync(string text);
        Task<string> UnprotectAsync(string text);
    }

    public class EncryptionGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IEncryptionGatewayService
    {
        public async Task<string> ProtectAsync(string text)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/protect",
                AppSettings.EncryptionMicroServiceApiKey!,
                true,
                new Dictionary<string, dynamic>
                {
                    { "text", text }
                }
            )).ProtectedValue!;
        }

        public async Task<string> UnprotectAsync(string text)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/unprotect",
                AppSettings.EncryptionMicroServiceApiKey!,
                true,
                new Dictionary<string, dynamic>
                {
                    { "text", text }
                }
            )).UnprotectedValue!;
        }
    }


}
