using Mapster;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventCommandService
    {
        Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request);
        Task<EventResponseModel?> CreateEventAsync(EventRequestModel request);
        Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request);
        Task<DeleteEventResponseModel> DeleteEventAsync(Guid id);
        Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request);
        Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request);
        Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request);
    }

    public class EventCommandService(IConfiguration configuration,
        IEventLookupService eventLookupService,
        IEventSecurityService eventSecurityService,
        IEventMutationRepository eventMutationRepository,
        IEventCacheRepository eventCacheRepository,
        IEventPublishingService eventPublishingService
        ) : ServiceBase(configuration), IEventCommandService
    {

        public async Task<EventResponseModel?> CreateEventAsync(EventRequestModel request)
        {
            var result = await eventMutationRepository.CreateEventAsync(request);
            if (result!.Success)
            {
                if (request.AutoAssign)
                {
                    var creationUser = await eventCacheRepository.GetUserByIdAsync(request.UserId!);
                    await AssignAsync(new AssignmentRequestModel
                    {
                        EventId = result.Id.ToString(),
                        UserId = request.UserId,
                        User = creationUser
                    });
                }
                result.Message = "Event created successfully.";
                return result;
            }
            return new EventResponseModel
            {
                Message = $"An unexpected error occurred. Please, try again."
            };
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            var result = await eventMutationRepository.UpdateEventAsync(request);
            if (result.Success)
            {
                result.Message = "Event updated successfully.";
                return result;
            }
            return new UpdateResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            var result = await eventMutationRepository.DeleteEventAsync(id);
            if (result.Success)
            {
                result.Message = "Event deleted successfully.";
                return result;
            }
            return new DeleteEventResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request)
        {
            var userId = string.Empty;
            if (!Guid.TryParse(request.UserId, out var _))
            {
                userId = await eventSecurityService.UnprotectAsync(request.UserId!);
            }
            var you = await eventCacheRepository.GetUserByIdAsync(userId);
            var users = (await eventLookupService.GetEventUsersAsync(request.EventId!)).ToList();
            if (users.Any(u => u.Admin) && you!.Admin)
            {
                return await Task.FromResult(new AssignmentResponseModel
                {
                    Message = "You cannot assign to this event as an administrator, because an event administrator already exists."
                });
            }
            
            var result = await AssignToEventAsync(request);
            if (result.Success)
            {
                result.Message = "Successfully assigned your user to event.";
                return result;
            }
            return new AssignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            var result = await eventMutationRepository.AssignToEventAsync(request);
            if (result.Success)
            {
                var evt = await eventLookupService.GetEventAsync(Guid.Parse(request.EventId!), true);
                var publishingItems = evt.Adapt<EventMessageRequestModel>();
                publishingItems.UserIds = evt!.Users!.Select(u => u.Id.ToString());
                await eventPublishingService.SendEventAsync(publishingItems);
                result.Message = "Successfully assigned user to event.";
                return result;
            }
            return new AssignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            var result = await eventMutationRepository.UnassignFromEventAsync(request);
            if (result.Success)
            {
                var evt = await eventLookupService.GetEventAsync(Guid.Parse(request.EventId!), true);
                var publishingItem = evt.Adapt<EventMessageRequestModel>();
                publishingItem.UserIds = evt!.Users!.Select(u => u.Id.ToString());
                await eventPublishingService.SendEventAsync(publishingItem);
                result.Message = "Successfully unassigned user from event.";
                return result;
            }
            return new UnassignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request)
        {
            request.EventId = await eventSecurityService.UnprotectAsync(request.EventId!);
            var result = await eventMutationRepository.SettleEventAsync(request);
            if (result.Success)
            {   
                await eventCacheRepository.DeleteActiveEventAsync(request.EventId!);
                result.Message = "The event was successfully settled.";
                return result;
            }
            return new SettleEventResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

       

       
    }
}
