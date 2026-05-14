using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentSecurityService
    {   
        public Task<string> ProtectAsync(string value);
        public Task<string> UnprotectAsync(string value);

        Task<bool> ValidateApiKey(string authorizationApiKey);
    }

    public class PaymentSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IPaymentSecurityService
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

        public async Task<bool> ValidateApiKey(string authorizationApiKey)
        {
            if (string.IsNullOrWhiteSpace(authorizationApiKey))
            {
                return false;
            }
            var apiKey = AppSettings.ApiKey;
            return apiKey == authorizationApiKey;
        }
    }
}
