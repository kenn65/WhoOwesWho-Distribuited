using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Payments
{
    public class PaymentsPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IPaymentsPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<UserPaymentResponseModel> GetUserPaymentsAsync(string eventId, string userId, bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var activeLowerCase = active.ToString().ToLowerInvariant();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{userId}/{activeLowerCase}");
            return await GetAsync<UserPaymentResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<UserBalanceResponseModel> GetUserBalanceAsync(string userId, string eventId, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBalanceBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{userId}/{eventId}");
            return await GetAsync<UserBalanceResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("create");
            return await PutAsync<CreatePaymentResponseModel, CreatePaymentRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        private async Task<string> GetPaymentsBaseAddressAsync() => appSettings.PaymentMicroserviceBaseAddress!;
        private async Task<string> GetPaymentsBalanceBaseAddressAsync() => appSettings.PaymentMicroserviceBalanceBaseAddress!;
        private async Task<string> GetPaymentsSettlementBaseAddressAsync() => appSettings.PaymentMicroserviceSettlementsBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.PaymentMicroserviceApiKey!;

      
    }
}
