using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Payments
{
    public class PaymentsPlugin(
        IConfiguration configuration, ITokenService tokenService, NavigationManager nav) : ApiPluginClientBase(configuration, tokenService, nav),
        IPaymentsPlugin
    {
        private readonly AppSettings appSettings = new(configuration);
        
        public async Task<UserHasPaymentsResponseModel> GetUserPaymentsAsync(Guid eventId, Guid userId, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var activeLowerCase = active.ToString().ToLowerInvariant();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{userId}/{activeLowerCase}");
            return await GetAsync<UserHasPaymentsResponseModel>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<UserBalanceResponseModel> GetUserBalanceAsync(Guid userId, Guid eventId)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBalanceBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{userId}/{eventId}");
            return await GetAsync<UserBalanceResponseModel>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync();
            return await PutAsync<CreatePaymentResponseModel, CreatePaymentRequestModel>
                (endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<PaymentsResponseModel> GetPaymentsDataAsync(Guid eventId, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{active}");
            return await GetAsync<PaymentsResponseModel>
                (endpoint, apiKey, true, applyToken: true);
        }

        public async Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync(Guid paymentId, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{paymentId}");
            return await GetAsync<PaymentDetailsResponseModel>
                (endpoint, 
                apiKey, 
                true, 
                new Dictionary<string, dynamic>
                {
                    { "active", active}
                },
                true);
        }

        public async Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync();
            return await PatchAsync<UpdatePaymentResponseModel, UpdatePaymentRequestModel>
                (endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetPaymentsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{paymentId}");
            return await DeleteAsync<DeletePaymentResponseModel>(endpoint, apiKey, true, applyToken: true);
        }


        private async Task<string> GetPaymentsBaseAddressAsync() => appSettings.PaymentMicroserviceBaseAddress!;
        private async Task<string> GetPaymentsBalanceBaseAddressAsync() => appSettings.PaymentMicroserviceBalanceBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.PaymentMicroserviceApiKey!;

       
    }
}
