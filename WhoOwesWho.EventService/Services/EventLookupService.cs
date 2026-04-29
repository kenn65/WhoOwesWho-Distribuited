using Mapster;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventLookupService
    {
        Task<EventResponseModel?> GetEventAsync(string id, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(string userId);
        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true);
        Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(string eventId, bool active = true);
    }
    public class EventLookupService(
        IConfiguration configuration,
        IEventSecurityService eventSecurityService,
        IEventQueryRepository eventQueryRepository,
        IEventCacheRepository eventCacheRepository
        ) : ServiceBase(configuration), IEventLookupService
    {


        public async Task<EventResponseModel?> GetEventAsync(string id, bool active = true)
        {
            var eventId = await eventSecurityService.UnprotectAsync(id); 
            return await eventQueryRepository.GetEventAsync(Guid.Parse(eventId), active);
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true)
        {
            var unprotectedUserId = await eventSecurityService.UnprotectAsync(userId);
            return await eventQueryRepository.GetEventByUserAsync(unprotectedUserId, active);
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(string userId)
        {
            var unprotectedUserId = await eventSecurityService.UnprotectAsync(userId);
            var user = await eventCacheRepository.GetUserByIdAsync(unprotectedUserId);
            IEnumerable<UserMessageResponseModel> users = new List<UserMessageResponseModel>
            {
                {
                    user! 
                }
            };
            var allEvents = await eventQueryRepository.GetEventsAsync();
            var filteredEvents = allEvents.Where(e => e.CreatedBy == user!.FullName).ToList();

            foreach (var item in filteredEvents)
            {
                if (item.Users == null || !item.Users.Any())
                {
                    item.Users = users;
                }
            }

            return filteredEvents;
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active)
        {
            return await eventQueryRepository.GetEventsAsync(active);
        }

        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true)
        {
            var userId = await eventSecurityService.UnprotectAsync(protectedUserId);
            var response = await eventQueryRepository.GetAssignmentAsync(protectedUserId, active);
            if (!response.Success) {
                response.Message = "No active events are available.";
            }
            return response;
        }

        public async Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(string eventId, bool active = true)
        {
            return await eventQueryRepository.GetEventUsersAsync(eventId, active);
        }
    }
}
