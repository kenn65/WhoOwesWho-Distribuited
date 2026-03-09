using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventLookupService
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active = true);
        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true);
        Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(string eventId, bool active = true);
    }
    public class EventLookupService(
        IConfiguration configuration,
        IEventSecurityService eventSecurityService,
        IEventQueryRepository eventQueryRepository
        ) : ServiceBase(configuration), IEventLookupService
    {


        public async Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true)
        {
            return await eventQueryRepository.GetEventAsync(id, active);
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(string userId,  bool active = true)
        {
            return await eventQueryRepository.GetEventByUserAsync(userId, active);
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active = true)
        {
            return await eventQueryRepository.GetEventsAsync(active);
        }

        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true)
        {
            var userId = await eventSecurityService.UnprotectAsync(protectedUserId);
            var result = await eventQueryRepository.GetAssignmentAsync(protectedUserId, active);
            if (!result.Success) {
                result.Message = "No active events are available.";
            }
            return result;
        }

        public async Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(string eventId, bool active = true)
        {
            return await eventQueryRepository.GetEventUsersAsync(eventId, active);
        }
    }
}
