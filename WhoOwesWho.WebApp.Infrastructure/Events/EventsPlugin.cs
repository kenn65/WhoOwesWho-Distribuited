using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Events
{
    public class EventsPlugin (IConfiguration configuration) : ApiPluginClientBase(configuration), IEventsPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(string userId, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetUserEventsBassAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync(string.Empty);
            return await GetAsync<IEnumerable<EventResponseModel>>(
                endpoint, 
                apiKey, 
                true, 
                new Dictionary<string, dynamic>
                {
                    { "userId", userId}
                }, 
                jwtToken);
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{active}");
            return await GetAsync<IEnumerable<EventResponseModel>>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> CreateEventAsync(EventRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync(string.Empty);
            return await PutAsync<EventResponseModel, EventRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> DeleteEventAsync(string id, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{id}");
            return await DeleteAsync<EventResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> GetEventAsync(string eventId, bool active, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{active}");
            return await GetAsync<EventResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventResponseModel> UpdateEventAsync(EventRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("update");
            return await PatchAsync<EventResponseModel, EventRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<EventUserAssignmentResponseModel> GetUserAssignmentAsync(string userId, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{userId}");
            return await GetAsync<EventUserAssignmentResponseModel>(endpoint, apiKey, true, null, jwtToken);
        }

        public async Task<EventAssignmentResponseModel> AssignToEventAsync(EventAssignmentRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("assign");
            return await PostAsync<EventAssignmentResponseModel, EventAssignmentRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        public async Task<EventUnassignmentResponseModel> UnassignFromEventAsync(EventUnassignmentRequestModel request, string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("unassign");
            return await PostAsync<EventUnassignmentResponseModel, EventUnassignmentRequestModel>(endpoint, request, apiKey, true, null, jwtToken);
        }

        private async Task<string> GetEventsBaseAddressAsync() => appSettings.EventMicroserviceEventsBaseAddress!;
        private async Task<string> GetEventUsersBaseAddressAsync() => appSettings.EventMicroserviceEventUsersBaseAddress!;
        private async Task<string> GetUserEventsBassAddressAsync() => appSettings.EventMicroserviceUserEventsBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.EventMicroserviceApiKey!;

        

       
    }
}
    

