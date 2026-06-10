using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventLookupService
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(Guid userId, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(Guid userId);
        Task<EventAssignmentModel> GetAssignmentAsync(Guid userId, bool active = true);
        Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(Guid eventId, bool active = true);
    }
    public class EventLookupService(
        IConfiguration configuration,
        IEventQueryRepository eventQueryRepository,
        IEventCacheRepository eventCacheRepository
        ) : ServiceBase(configuration), IEventLookupService
    {
        public async Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true)
        {
            return await eventQueryRepository.GetEventAsync(id, active);
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(Guid userId, bool active = true)
        {
            return await eventQueryRepository.GetEventByUserAsync(userId, active);
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(Guid userId)
        {
            var user = await eventCacheRepository.GetUserByIdAsync(userId.ToString());
            var allEvents = await eventQueryRepository.GetEventsAsync();
            var filteredEvents = allEvents.Where(e => e.CreatedBy == user!.FullName).OrderByDescending(e => e.StartDateIso).ToList();

            //foreach (var item in filteredEvents)
            //{
            //    if (item.Users == null || !item.Users.Any())
            //    {
            //        item.Users = users;
            //    }
            //}
            return filteredEvents.Any() ? filteredEvents : [];
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active)
        {
            return (await eventQueryRepository.GetEventsAsync(active)).OrderByDescending(e => e.StartDateIsoYmd);
        }

        public async Task<EventAssignmentModel> GetAssignmentAsync(Guid userId, bool active = true)
        {
            var response = await eventQueryRepository.GetAssignmentAsync(userId, active);
            response.Success = true;
            return response;
        }

        public async Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(Guid eventId, bool active = true)
        {
            return await eventQueryRepository.GetEventUsersAsync(eventId, active);
        }
    }
}
