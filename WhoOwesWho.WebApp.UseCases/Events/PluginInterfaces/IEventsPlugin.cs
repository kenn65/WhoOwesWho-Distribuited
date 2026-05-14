using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;

namespace WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces
{
    public interface IEventsPlugin
    {
        Task<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>> GetEventsAsync(Guid userId, string jwtToken);
        Task<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>> GetEventsAsync(bool active, string jwtToken);
        Task<EventResponseModel> CreateEventAsync(EventRequestModel request, string jwtToken);
        Task<EventResponseModel> DeleteEventAsync(Guid id, string jwtToken);
        Task<EventResponseModel> GetEventAsync(Guid eventId, bool active, string jwtToken);
        Task<EventResponseModel> UpdateEventAsync(EventRequestModel request, string jwtToken);
        Task<EventUserAssignmentResponseModel> GetUserAssignmentAsync(Guid userId, string jwtToken);
        Task<EventAssignmentResponseModel> AssignToEventAsync(EventAssignmentRequestModel request, string jwtToken);
        Task<EventUnassignmentResponseModel> UnassignFromEventAsync(EventUnassignmentRequestModel request, string jwtToken);
    }
}
