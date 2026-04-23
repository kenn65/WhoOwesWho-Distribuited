using Microsoft.EntityFrameworkCore;
using WhoOwesWho.EventService.EfCore.Context;
using WhoOwesWho.EventService.Models;
using Mapster;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Repositories
{
    public interface IEventQueryRepository
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync();
        Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(string eventId, bool active);
        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true);
    }

    public class EventQueryRepository(
        EventDbContext context,
        IEventSecurityService eventSecurityService,
        IUserCacheService userCacheService
        ) : IEventQueryRepository
    {
        public async Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true)
        {
            var entity = await context.Events.Where(e => e.Id == id && e.Settled == !active)
                .ProjectToType<EventResponseModel>().FirstOrDefaultAsync();
            var userIds = await context.EventAssingments.Where(ea => ea.EventId == entity!.Id).Select(ea => ea.UserId).ToListAsync();
            entity!.Users = await GetEventAssignmentUsersAsync(userIds);
            return entity;
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true)
        {
            var entityAssignments = await context.EventAssingments.Where(ea => ea.UserId.ToString() == userId).FirstOrDefaultAsync();
            if (entityAssignments is null)
            {
                return new EventResponseModel();
            }
            var userIds = await context.EventAssingments.Where(ea => ea.EventId == entityAssignments!.EventId).Select(ea => ea.UserId).ToListAsync();
            var entity = await context.Events.Where(e => e.Id == entityAssignments!.EventId && e.Settled == !active)
              .ProjectToType<EventResponseModel>().FirstOrDefaultAsync();
            entity!.Users = await GetEventAssignmentUsersAsync(userIds);
            return entity;
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync()
        {
            var entities = await context.Events.ProjectToType<EventResponseModel>().ToListAsync();
            foreach (var entity in entities)
            {
                var userIds = await context.EventAssingments.Where(ea => ea.EventId == entity.Id).Select(ea => ea.UserId).ToListAsync();
                entity.Users = await GetEventAssignmentUsersAsync(userIds);
            }
            return [.. entities];
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active = true)
        {
            var entities = await context.Events.Where(e => e.Settled == !active).ProjectToType<EventResponseModel>().ToListAsync();
            foreach (var entity in entities)
            {
                var userIds = await context.EventAssingments.Where(ea => ea.EventId == entity.Id).Select(ea => ea.UserId).ToListAsync();
                entity.Users = await GetEventAssignmentUsersAsync(userIds);
            }
            return [.. entities];
        }

        public async Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(string eventId, bool active)
        {
            try
            {
                var eventGuid = Guid.Parse(eventId);

                var thisEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == eventGuid && e.Settled == !active);
                var userIds = await context.EventAssingments.Where(ea => ea.EventId == thisEvent!.Id).Select(ea => ea.UserId).ToListAsync();
                var users = await GetEventAssignmentUsersAsync(userIds);
                return [.. users];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true)
        {
            var userId = await eventSecurityService.UnprotectAsync(protectedUserId);
            var assignment = await context.EventAssingments.Where(ea => ea.UserId.ToString() == userId).FirstOrDefaultAsync();
            if (assignment is null)
            {
                return new EventAssignmentModel();
            }
            var entity = await context.Events.Where(e => e.Id == assignment!.EventId && e.Settled == !active).FirstOrDefaultAsync();
            if (entity is null)
            {
                return new EventAssignmentModel();
            }
            return new EventAssignmentModel
            {
                EventId = entity!.Id,
                User = await userCacheService.GetUserAsync(userId)
            };
        }

        private async Task<IEnumerable<UserMessageResponseModel>> GetEventAssignmentUsersAsync(IEnumerable<Guid> userIds)
        {
            var users = await Task.WhenAll(userIds.Select(async userId =>
            {
                return await userCacheService.GetUserAsync(userId.ToString());
            }));
            return [.. users!];
        }
    }
}
