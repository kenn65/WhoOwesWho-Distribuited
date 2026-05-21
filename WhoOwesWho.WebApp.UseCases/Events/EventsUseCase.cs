using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Events
{
    public interface IEventsUseCase
    {
        Task<EventResponseModel> ExecuteCreateEventAsync(EventRequestModel request, string jwtToken);
        Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(Guid userId, string jwtToken);
        Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(bool active, string jwtToken);
        Task<IEnumerable<UserMessageResponseModel>> ExecuteGetEventUsersAsync(Guid eventId, bool active, string jwtToken);
        Task<EventResponseModel> ExecuteDeleteEventAsync(Guid eventId, string jwtToken);
        Task<EventResponseModel> ExecuteGetEventAsync(Guid eventId, bool active, string jwtToken);
        Task<EventResponseModel> ExecuteUpdateEventAsync(EventRequestModel request, string jwtToken);
        Task<EventUserAssignmentResponseModel> ExecuteGetUserAssignmentAsync(Guid userId, bool active,string jwtToken);
        Task<EventAssignmentResponseModel> ExecuteAssignToEventAsync(EventAssignmentRequestModel request, string jwtToken);
        Task<EventUnassignmentResponseModel> ExecuteUnassignFromEventAsync(EventUnassignmentRequestModel request, string jwtToken);
        Task<SettleEventResponseModel> ExecuteSettleEventAsync(SettleEventRequestModel request, string jwtToken);
        Task<SettleEventResponseModel> ExecuteUnsettleEventAsync(SettleEventRequestModel request, string jwtToken);
    }

    public class EventsUseCase(IEventsPlugin eventsPlugin) : IEventsUseCase
    {
        public async Task<EventResponseModel> ExecuteCreateEventAsync(EventRequestModel request, string jwtToken)
        {
            return await eventsPlugin.CreateEventAsync(request, jwtToken);
        }

        public async Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(Guid userId, string jwtToken)
        {
            return (await eventsPlugin.GetEventsAsync(userId, jwtToken))?.Data!;
        }

        public async Task<IEnumerable<EventResponseModel>> ExecuteGetEventsAsync(bool active, string jwtToken)
        {
            return (await eventsPlugin.GetEventsAsync(active, jwtToken))?.Data!;
        }

        public async Task<IEnumerable<UserMessageResponseModel>> ExecuteGetEventUsersAsync(Guid eventId, bool active, string jwtToken)
        {
            return (await eventsPlugin.GetEventUsersAsync(eventId, active, jwtToken))?.Data!;
        }

        public async Task<EventResponseModel> ExecuteDeleteEventAsync(Guid eventId, string jwtToken)
        {
            return await eventsPlugin.DeleteEventAsync(eventId, jwtToken);
        }

        public async Task<EventResponseModel> ExecuteGetEventAsync(Guid eventId, bool active, string jwtToken)
        {
            return await eventsPlugin.GetEventAsync(eventId, active, jwtToken);
        }

        public async Task<EventResponseModel> ExecuteUpdateEventAsync(EventRequestModel request, string jwtToken)
        {   
            return await eventsPlugin.UpdateEventAsync(request, jwtToken);
        }

        public async Task<EventUserAssignmentResponseModel> ExecuteGetUserAssignmentAsync(Guid userId, bool active, string jwtToken)
        {
           return await eventsPlugin.GetUserAssignmentAsync(userId, active, jwtToken);
        }

        public async Task<EventAssignmentResponseModel> ExecuteAssignToEventAsync(EventAssignmentRequestModel request, string jwtToken)
        {
            return await eventsPlugin.AssignToEventAsync(request, jwtToken);
        }

        public async Task<EventUnassignmentResponseModel> ExecuteUnassignFromEventAsync(EventUnassignmentRequestModel request, string jwtToken)
        {
            return await eventsPlugin.UnassignFromEventAsync(request, jwtToken);
        }

        public async Task<SettleEventResponseModel> ExecuteSettleEventAsync(SettleEventRequestModel request, string jwtToken)
        {
            return await eventsPlugin.SettleEventAsync(request, jwtToken);
        }

        public async Task<SettleEventResponseModel> ExecuteUnsettleEventAsync(SettleEventRequestModel request, string jwtToken)
        {
            return await eventsPlugin.UnsettleEventAsync(request, jwtToken);
        }

        
    }
}
