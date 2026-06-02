using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Events
{
    public interface IEventsUseCase
    {
        Task<EventResponseModel> ExecuteCreateEventAsync(EventRequestModel request);
        Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(Guid userId);
        Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(bool active);
        Task<IEnumerable<UserMessageResponseModel>> ExecuteGetEventUsersAsync(Guid eventId, bool active);
        Task<EventResponseModel> ExecuteDeleteEventAsync(Guid eventId);
        Task<EventResponseModel> ExecuteGetEventAsync(Guid eventId, bool active);
        Task<EventResponseModel> ExecuteUpdateEventAsync(EventRequestModel request);
        Task<EventUserAssignmentResponseModel> ExecuteGetUserAssignmentAsync(Guid userId, bool active);
        Task<EventAssignmentResponseModel> ExecuteAssignToEventAsync(EventAssignmentRequestModel request);
        Task<EventUnassignmentResponseModel> ExecuteUnassignFromEventAsync(EventUnassignmentRequestModel request);
        Task<SettleEventResponseModel> ExecuteSettleEventAsync(SettleEventRequestModel request);
        Task<SettleEventResponseModel> ExecuteUnsettleEventAsync(SettleEventRequestModel request);
    }

    public class EventsUseCase(IEventsPlugin eventsPlugin) : IEventsUseCase
    {
        public async Task<EventResponseModel> ExecuteCreateEventAsync(EventRequestModel request)
        {
            return await eventsPlugin.CreateEventAsync(request);
        }

        public async Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(Guid userId)
        {
            return (await eventsPlugin.GetEventsAsync(userId))?.Data!;
        }

        public async Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(bool active)
        {
            return (await eventsPlugin.GetEventsAsync(active))?.Data!;
        }

        public async Task<IEnumerable<UserMessageResponseModel>> ExecuteGetEventUsersAsync(Guid eventId, bool active)
        {
            return (await eventsPlugin.GetEventUsersAsync(eventId, active))?.Data!;
        }

        public async Task<EventResponseModel> ExecuteDeleteEventAsync(Guid eventId)
        {
            return await eventsPlugin.DeleteEventAsync(eventId);
        }

        public async Task<EventResponseModel> ExecuteGetEventAsync(Guid eventId, bool active)
        {
            return await eventsPlugin.GetEventAsync(eventId, active);
        }

        public async Task<EventResponseModel> ExecuteUpdateEventAsync(EventRequestModel request)
        {   
            return await eventsPlugin.UpdateEventAsync(request);
        }

        public async Task<EventUserAssignmentResponseModel> ExecuteGetUserAssignmentAsync(Guid userId, bool active)
        {
           return await eventsPlugin.GetUserAssignmentAsync(userId, active);
        }

        public async Task<EventAssignmentResponseModel> ExecuteAssignToEventAsync(EventAssignmentRequestModel request)
        {
            return await eventsPlugin.AssignToEventAsync(request);
        }

        public async Task<EventUnassignmentResponseModel> ExecuteUnassignFromEventAsync(EventUnassignmentRequestModel request)
        {
            return await eventsPlugin.UnassignFromEventAsync(request);
        }

        public async Task<SettleEventResponseModel> ExecuteSettleEventAsync(SettleEventRequestModel request)
        {
            return await eventsPlugin.SettleEventAsync(request);
        }

        public async Task<SettleEventResponseModel> ExecuteUnsettleEventAsync(SettleEventRequestModel request)
        {
            return await eventsPlugin.UnsettleEventAsync(request);
        }

        
    }
}
