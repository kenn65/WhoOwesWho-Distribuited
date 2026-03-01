using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventLookupService
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, string token, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(string userId, string token, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(string token, bool active = true);
        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, string token, bool active = true);
        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool active = true);
    }
    public class EventLookupService(
        IConfiguration configuration,
        IEventQueryRepository eventQueryRepository
        ) : ServiceBase(configuration), IEventLookupService
    {


        public async Task<EventResponseModel?> GetEventAsync(Guid id, string token, bool active = true)
        {
            return await eventQueryRepository.GetEventAsync(id, token, active);
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(string userId, string token, bool active = true)
        {
            return await eventQueryRepository.GetEventByUserAsync(userId, token, active);
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(string token, bool active = true)
        {
            return await eventQueryRepository.GetEventsAsync(token, active);
        }

        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, string token, bool active = true)
        {
            return await eventQueryRepository.GetAssignmentAsync(protectedUserId, token, active);
        }

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool active = true)
        {
            return await eventQueryRepository.GetEventUsersAsync(eventId, token, active);
        }
    }
}
