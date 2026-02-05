using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Services.Gateways
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
                false,
                new Dictionary<string, dynamic>()
            )).UnprotectedValue!;
        }
    }
}
