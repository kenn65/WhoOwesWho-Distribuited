using Microsoft.EntityFrameworkCore;
using WhoOwesWho.EventService.EfCore.Context;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Models.Models;
using Mapster;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Repositories
{
    public interface IEventQueryRepository
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, string token, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(string userId, string token, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(string token, bool active = true);
        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool active);
        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, string token, bool active = true);
    }

    public class EventQueryRepository(EventDbContext context, IEventSecurityService eventSecurityService, IUserGatewayService userGatewayService) : IEventQueryRepository
    {
        public async Task<EventResponseModel?> GetEventAsync(Guid id, string token, bool active = true)
        {
            var entity = await context.Events.Where(e => e.Id == id && e.Settled == !active)
                .ProjectToType<EventResponseModel>().FirstOrDefaultAsync();
            var userIds = await context.EventAssingments.Where(ea => ea.EventId == entity!.Id).Select(ea => ea.UserId).ToListAsync();
            entity!.Users = await GetEventAssignmentUsersAsync(userIds, token);
            return entity;
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(string userId, string token, bool active = true)
        {
            var unprotectedUserId = await eventSecurityService.UnprotectAsync(userId);
                var entityAssignments = await context.EventAssingments.Where(ea => ea.UserId.ToString() == unprotectedUserId).FirstOrDefaultAsync();
            if (entityAssignments is null)
            {
                return new EventResponseModel();
            }
            var userIds = await context.EventAssingments.Where(ea => ea.EventId == entityAssignments!.EventId).Select(ea => ea.UserId).ToListAsync();
            var entity = await context.Events.Where(e => e.Id == entityAssignments!.EventId && e.Settled == !active)
              .ProjectToType<EventResponseModel>().FirstOrDefaultAsync();
            entity!.Users = await GetEventAssignmentUsersAsync(userIds, token);
            return entity;
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(string token, bool active = true)
        {
            var entities = await context.Events.Where(e => e.Settled == !active).ProjectToType<EventResponseModel>().ToListAsync();
            foreach (var entity in entities)
            {
                var userIds = await context.EventAssingments.Where(ea => ea.EventId == entity.Id).Select(ea => ea.UserId).ToListAsync();
                entity.Users = await GetEventAssignmentUsersAsync(userIds, token);
            }
            return [.. entities];
        }

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool active)
        {
            try
            {
                var eventGuid = Guid.Parse(eventId);

                var thisEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == eventGuid && e.Settled == !active);
                var userIds = await context.EventAssingments.Where(ea => ea.EventId == thisEvent!.Id).Select(ea => ea.UserId).ToListAsync();
                var users = await GetEventAssignmentUsersAsync(userIds, token);
                return [.. users];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, string token, bool active = true)
        {
            var userId = await eventSecurityService.UnprotectAsync(protectedUserId);
            var assignment = await context.EventAssingments.Where(ea => ea.UserId.ToString() == userId).FirstOrDefaultAsync();
            if (assignment is null)
            {
                return new EventAssignmentModel();
            }
            var entity = await context.Events.Where(e => e.Id == assignment!.EventId && e.Settled == !active).FirstOrDefaultAsync();
            return new EventAssignmentModel
            {
                EventId = entity!.Id,
                User = await userGatewayService.GetAuthorizedUserAsync(protectedUserId, token, true, false)
            };
        }

        private async Task<IEnumerable<UserModel>> GetEventAssignmentUsersAsync(IEnumerable<Guid> userIds, string token)
        {
            var users = await Task.WhenAll(userIds.Select(async userId =>
            {
                var protectedUserId = await eventSecurityService.ProtectAsync(userId.ToString());
                return await userGatewayService.GetAuthorizedUserAsync(protectedUserId, token, true, false);
            }));
            return [.. users];
        }
    }
}
