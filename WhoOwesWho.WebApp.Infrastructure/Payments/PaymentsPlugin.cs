using Mapster;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Payments
{
    public class PaymentsPlugin(
        IConfiguration configuration) : ApiPluginClientBase(configuration),
        IPaymentsPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<UserHasPaymentsResponseModel> GetUserPaymentsAsync(Guid eventId, Guid userId, bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var activeLowerCase = active.ToString().ToLowerInvariant();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{userId}/{activeLowerCase}");
            return await GetAsync<UserHasPaymentsResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<UserBalanceResponseModel> GetUserBalanceAsync(Guid userId, Guid eventId, string jwtToken)
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
            return await PutAsync<CreatePaymentResponseModel, CreatePaymentRequestModel>
                (endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<PaymentsResponseModel> GetPaymentsDataAsync(Guid eventId, bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{active}");
            return await GetAsync<PaymentsResponseModel>
                (endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync(Guid paymentId, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{paymentId}");
            return await GetAsync<PaymentDetailsResponseModel>
                (endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("update");
            return await PatchAsync<UpdatePaymentResponseModel, UpdatePaymentRequestModel>
                (endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"delete/{paymentId}");
            return await DeleteAsync<DeletePaymentResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }


        private async Task<string> GetPaymentsBaseAddressAsync() => appSettings.PaymentMicroserviceBaseAddress!;
        private async Task<string> GetPaymentsBalanceBaseAddressAsync() => appSettings.PaymentMicroserviceBalanceBaseAddress!;
        private async Task<string> GetPaymentsSettlementBaseAddressAsync() => appSettings.PaymentMicroserviceSettlementsBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.PaymentMicroserviceApiKey!;

       
    }
}
