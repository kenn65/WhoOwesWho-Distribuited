using Microsoft.Extensions.Configuration;
using System.Net;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Events
{
    public class EventsPlugin (IConfiguration configuration) : ApiPluginClientBase(configuration), IEventsPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var activeString = active.ToString().ToLowerInvariant();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await CreateEndpointAsync(baseAddress, $"{activeString}");
            return await GetAsync<IEnumerable<EventResponseModel>>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> CreateEventAsync(EventRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await CreateEndpointAsync(baseAddress, string.Empty);
            return await PutAsync<EventResponseModel, EventRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> DeleteEventAsync(string id, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await CreateEndpointAsync(baseAddress, $"{id}");
            return await DeleteAsync<EventResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> GetEventAsync(string eventId, bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await CreateEndpointAsync(baseAddress, $"{eventId}/{active}");
            return await GetAsync<EventResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> UpdateEventAsync(EventRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await CreateEndpointAsync(baseAddress, "update");
            return await PatchAsync<EventResponseModel, EventRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<EventAssignmentResponseModel> GetUserAssignmentAsync(string userId, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await CreateEndpointAsync(baseAddress, $"{userId}");
            return await GetAsync<EventAssignmentResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        private async Task<string> GetEventsBaseAddressAsync() => appSettings.EventMicroserviceEventsBaseAddress!;
        private async Task<string> GetEventUsersBaseAddressAsync() => appSettings.EventMicroserviceEventUsersBaseAddress!;
        private async Task<string> GetUserEventsBassAddressAsync() => appSettings.EventMicroserviceUserEventsBaseAddress!;

        private async Task<string> GetApiKeyAsync() => appSettings.EventMicroserviceApiKey!;

        private async Task<string> CreateEndpointAsync(string baseAddress, string trailingPath)
        {
            if (string.IsNullOrWhiteSpace(trailingPath))
            {
                return baseAddress;
            }
            return $"{baseAddress}/{trailingPath}";
        }

        
    }
}
    

