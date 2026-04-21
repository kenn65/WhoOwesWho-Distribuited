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
        Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request);
    }

    public class EventMutationRepository(
        EventDbContext context, 
        IEventQueryRepository eventQueryRepository, 
        IEventCacheRepository eventCacheRepository,
        ICurrencyGatewayService currencyGatewayService, 
        IEventSecurityService eventSecurityService
        ) : IEventMutationRepository
    {
        public async Task<EventResponseModel?> CreateEventAsync(EventRequestModel request)
        {
            try
            {
                request.Id = Guid.NewGuid();
                var userId = string.Empty;
                //if (!Guid.TryParse(request.UserId, out var _))
                //{
                //    userId = await eventSecurityService.UnprotectAsync(request.UserId!);
                //}
                //var creationUser = await eventCacheRepository.GetUserByIdAsync(userId);
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
            catch 
            {
                return new EventResponseModel();
            }
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            try
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
            catch 
            {
                return new UpdateResponseModel();
            }
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            try
            {
                await context.Events
                       .Where(x => x.Id == id)
                       .ExecuteDeleteAsync();

                return new DeleteEventResponseModel
                {
                    Success = true,
                };
            }
            catch 
            {
                return new DeleteEventResponseModel();
            }
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out var _))
                {
                    request.UserId = await eventSecurityService.UnprotectAsync(request.UserId!);
                }
                var user = await eventCacheRepository.GetUserByIdAsync(request.UserId!);
                request.UserId = user?.Id.ToString();
                var entity = new EventAssignments
                {
                    EventId = Guid.Parse(request.EventId!),
                    UserId = Guid.Parse(request.UserId!)
                };
                await context.AddAsync(entity);
                await context.SaveChangesAsync();
                return new AssignmentResponseModel
                {
                    Success = true,
                };
            }
            catch 
            {
                return new AssignmentResponseModel();
            }
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            try
            {
                var userId = await eventSecurityService.UnprotectAsync(request.UserId!);
                await context.EventAssingments
                       .Where(x => x.EventId == Guid.Parse(request.EventId!) && x.UserId == Guid.Parse(userId))
                       .ExecuteDeleteAsync();
                return new UnassignmentResponseModel
                {
                    Success = true
                };
            }
            catch 
            {
                return new UnassignmentResponseModel();
               
            }
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request)
        {
            try
            {
                await context.Events.Where(e => e.Id == Guid.Parse(request.EventId!)).ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.Settled, true)
                );
                return new SettleEventResponseModel
                {
                    Success = true
                };
            }
            catch 
            {
                return new SettleEventResponseModel();
            }
        }
    }
}
