using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;

namespace WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces
{
    public interface IEventsPlugin
    {
        Task<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>> GetEventsAsync(Guid userId);
        Task<EnumerableWrapperResponseModel<IEnumerable<EventResponseModel>>> GetEventsAsync(bool active);
        Task<EnumerableWrapperResponseModel<IEnumerable<UserMessageResponseModel>>> GetEventUsersAsync(Guid eventId, bool active);
        Task<EventResponseModel> CreateEventAsync(EventRequestModel request);
        Task<EventResponseModel> DeleteEventAsync(Guid id);
        Task<EventResponseModel> GetEventAsync(Guid eventId, bool active);
        Task<EventResponseModel> UpdateEventAsync(EventRequestModel request);
        Task<EventUserAssignmentResponseModel> GetUserAssignmentAsync(Guid userId, bool active);
        Task<EventAssignmentResponseModel> AssignToEventAsync(EventAssignmentRequestModel request);
        Task<EventUnassignmentResponseModel> UnassignFromEventAsync(EventUnassignmentRequestModel request);
        Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request);
        Task<SettleEventResponseModel> UnsettleEventAsync(SettleEventRequestModel request);
    }
}
