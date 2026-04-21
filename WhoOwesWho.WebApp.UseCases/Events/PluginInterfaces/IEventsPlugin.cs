using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;

namespace WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces
{
    public interface IEventsPlugin
    {
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active, string jwtToken);
        Task<EventResponseModel> CreateEventAsync(EventRequestModel request, string jwtToken);
        Task<EventResponseModel> DeleteEventAsync(string id, string jwtToken);
        Task<EventResponseModel> GetEventAsync(string eventId, bool active, string jwtToken);
        Task<EventResponseModel> UpdateEventAsync(EventRequestModel request, string jwtToken);
        Task<EventAssignmentResponseModel> GetUserAssignmentAsync(string userId, string jwtToken);
    }
}
