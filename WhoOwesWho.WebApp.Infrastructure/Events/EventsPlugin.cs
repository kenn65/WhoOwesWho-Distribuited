using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Events
{
    public class EventsPlugin(IConfiguration configuration, ITokenService tokenService, NavigationManager nav)
        : ApiPluginClientBase(configuration, tokenService, nav), IEventsPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>> GetEventsAsync(string createdBy, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetUserEventsBassAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{createdBy}");
            return await GetAsync<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>>(endpoint, apiKey, true,
                new Dictionary<string, dynamic>
                {
                    { "active", active}
                },
                true);
        }

        public async Task<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>> GetEventsAsync(bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{active}");
            return await GetAsync<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<EventResponseModel> CreateEventAsync(EventRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync();
            return await PutAsync<EventResponseModel, EventRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<EventResponseModel> DeleteEventAsync(Guid id)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{id}");
            return await DeleteAsync<EventResponseModel>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<EventResponseModel> GetEventAsync(Guid eventId, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{active}");
            return await GetAsync<EventResponseModel>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<EventResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync();
            return await PatchAsync<EventResponseModel, EventRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<EnumerableWrapperResponseModel<IEnumerable<UserMessageResponseModel>>> GetEventUsersAsync(Guid eventId, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{eventId}/{active}");
            return await GetAsync<EnumerableWrapperResponseModel<IEnumerable<UserMessageResponseModel>>>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<EventUserAssignmentResponseModel> GetUserAssignmentAsync(Guid userId, bool active)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"{userId}");
            return await GetAsync<EventUserAssignmentResponseModel>(
                endpoint,
                apiKey,
                true,
                new Dictionary<string, dynamic>
                {
                    { "active", active }
                },
                true);
        }

        public async Task<EventAssignmentResponseModel> AssignToEventAsync(EventAssignmentRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("assign");
            return await PostAsync<EventAssignmentResponseModel, EventAssignmentRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<EventUnassignmentResponseModel> UnassignFromEventAsync(EventUnassignmentRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventUsersBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("unassign");
            return await PostAsync<EventUnassignmentResponseModel, EventUnassignmentRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("settle");
            return await PostAsync<SettleEventResponseModel, SettleEventRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<SettleEventResponseModel> UnsettleEventAsync(SettleEventRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetEventsBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("unsettle");
            return await PostAsync<SettleEventResponseModel, SettleEventRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        private async Task<string> GetEventsBaseAddressAsync() => appSettings.EventMicroserviceEventsBaseAddress!;
        private async Task<string> GetEventUsersBaseAddressAsync() => appSettings.EventMicroserviceEventUsersBaseAddress!;
        private async Task<string> GetUserEventsBassAddressAsync() => appSettings.EventMicroserviceUserEventsBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.EventMicroserviceApiKey!;


    }
}


