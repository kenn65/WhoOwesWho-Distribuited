using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services.Gateways
{

    public interface IEncryptionGatewayService
    {
        Task<string> ProtectAsync(string text, bool encode);
        Task<string> UnprotectAsync(string text, bool encode);
    }

    public class EncryptionGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IEncryptionGatewayService
    {
        public async Task<string> ProtectAsync(string text, bool encode)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/protect/{text}",
                AppSettings.EncryptionMicroServiceApiKey,
                encode,
                parameters: null
            )).ProtectedValue!;
        }

        public async Task<string> UnprotectAsync(string text, bool encode)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/unprotect/{text}",
                AppSettings.EncryptionMicroServiceApiKey,
                encode,
                parameters: null
            )).UnprotectedValue!;
        }
    }
}
