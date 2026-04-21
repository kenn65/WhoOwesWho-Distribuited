using System.Globalization;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.UseCases.Events
{
    public interface IEventsUseCase
    {
        Task<IEnumerable<EventResponseModel>> ExecuteAsync(bool active, string jwtToken);
        Task<EventResponseModel> ExecuteAsync(EventRequestModel request, string jwtToken);
        Task<EventResponseModel> ExecuteAsync(Guid id, string jwtToken);
        Task<EventResponseModel> ExecuteAsync(string id, bool active, string jwtToken);
        Task<EventResponseModel> ExecuteAsync(EventRequestModel request, string jwtToken, bool dummy = false);
    }

    public class EventsUseCase(IEventsPlugin eventsPlugin, IProtectionUseCase protectionUseCase) : IEventsUseCase
    {
        public async Task<EventResponseModel> ExecuteAsync(EventRequestModel request, string jwtToken)
        {
            return await eventsPlugin.CreateEventAsync(request, jwtToken);
        }

        public async Task<IEnumerable<EventResponseModel>> ExecuteAsync(bool active, string jwtToken)
        {
            return await eventsPlugin.GetEventsAsync(active, jwtToken);
        }

        public async Task<EventResponseModel> ExecuteAsync(Guid id, string jwtToken)
        {
            var eventId = await protectionUseCase.ExecuteProtectAsync(id.ToString());
            return await eventsPlugin.DeleteEventAsync(eventId, jwtToken);
        }

        public async Task<EventResponseModel> ExecuteAsync(string id, bool active, string jwtToken)
        {
            var eventId = await protectionUseCase.ExecuteProtectAsync(id);
            return await eventsPlugin.GetEventAsync(eventId, active, jwtToken);
        }

        public async Task<EventResponseModel> ExecuteAsync(EventRequestModel request, string jwtToken, bool dummy = false)
        {
            return await eventsPlugin.UpdateEventAsync(request, jwtToken);
        }
    }
}
