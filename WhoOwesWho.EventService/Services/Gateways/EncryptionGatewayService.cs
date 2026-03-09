using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Shared.Models;

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
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/protect/{text}",
                AppSettings.EncryptionMicroServiceApiKey!,
                false,
                new Dictionary<string, dynamic>()
            )).ProtectedValue!;
        }

        public async Task<string> UnprotectAsync(string text)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/unprotect/{text}",
                AppSettings.EncryptionMicroServiceApiKey!,
                true,
                new Dictionary<string, dynamic>()
                
                
            )).UnprotectedValue!;
        }
    }


}
