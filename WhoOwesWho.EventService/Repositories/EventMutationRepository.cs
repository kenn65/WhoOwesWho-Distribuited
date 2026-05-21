using Microsoft.EntityFrameworkCore;
using WhoOwesWho.EventService.EfCore.Context;
using WhoOwesWho.EventService.EfCore.DataModels;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Repositories
{
    public interface IEventMutationRepository
    {
        Task<EventResponseModel?> CreateEventAsync(EventRequestModel request);
        Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request);
        Task<DeleteEventResponseModel> DeleteEventAsync(Guid id);
        Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request);
        Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request);
        Task<SettleEventResponseModel> SettleEventAsync(Guid eventId);
        Task<SettleEventResponseModel> UnsettleEventAsync(Guid eventId);
    }

    public class EventMutationRepository(
        EventDbContext context,
        IEventQueryRepository eventQueryRepository,
        IEventCacheRepository eventCacheRepository,
        ICurrencyGatewayService currencyGatewayService
        ) : IEventMutationRepository
    {
        public async Task<EventResponseModel?> CreateEventAsync(EventRequestModel request)
        {
            request.Id = Guid.NewGuid();
            var userId = string.Empty;
            request.CurrencySymbol = await currencyGatewayService.GetCurrencySymbolAsync(request.Currency!, request.Token!);

            var entity = new Events
            {
                Id = request.Id,
                CreatedBy = request!.CreatedBy,
                Name = request.Name,
                Location = request.Location,
                Currency = request.Currency,
                CurrencySymbol = request.CurrencySymbol,
                StartDate = request.StartDateTicks,
                Settled = request.Settled
            };

            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            var response = await eventQueryRepository.GetEventAsync(request.Id, true);

            response!.Success = true;
            return response;
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            request.CurrencySymbol = await currencyGatewayService.GetCurrencySymbolAsync(request.Currency!, request.Token!);

            var entity = new Events
            {
                Name = request.Name,
                Location = request.Location,
                Currency = request.Currency,
                CurrencySymbol = request.CurrencySymbol,
                StartDate = request.StartDateTicks,
                Settled = request.Settled,
            };
            await context.Events.Where(e => e.Id == request.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Name, entity.Name)
                .SetProperty(e => e.Location, entity.Location)
                .SetProperty(e => e.Currency, entity.Currency)
                .SetProperty(e => e.CurrencySymbol, entity.CurrencySymbol)
                .SetProperty(e => e.StartDate, entity.StartDate)
                .SetProperty(e => e.Settled, entity.Settled)
            );

            return new UpdateResponseModel
            {
                Success = true,
            };
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            await context.Events
                   .Where(x => x.Id == id)
                   .ExecuteDeleteAsync();

            return new DeleteEventResponseModel
            {
                Success = true,
            };
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            var user = await eventCacheRepository.GetUserByIdAsync(request.UserId.ToString());
            request.UserId = user!.Id;
            var entity = new EventAssignments
            {
                EventId = request.EventId,
                UserId = request.UserId
            };
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
            return new AssignmentResponseModel
            {
                Success = true,
            };
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            await context.EventAssingments
                   .Where(x => x.EventId == request.EventId! && x.UserId == request.UserId)
                   .ExecuteDeleteAsync();
            return new UnassignmentResponseModel
            {
                Success = true
            };
        }


        public async Task<SettleEventResponseModel> SettleEventAsync(Guid eventId)
        {
            await context.Events.Where(e => e.Id == eventId!).ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Settled, true)
            );
            return new SettleEventResponseModel
            {
                Success = true
            };
        }

        public async Task<SettleEventResponseModel> UnsettleEventAsync(Guid eventId)
        {
            await context.Events.Where(e => e.Id == eventId!).ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Settled, false)
            );
            return new SettleEventResponseModel
            {
                Success = true
            };
        }
    }
}
