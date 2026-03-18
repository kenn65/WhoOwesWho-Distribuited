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
            var response = await eventMutationRepository.CreateEventAsync(request);
            if (response!.Success)
            {
                if (request.AutoAssign)
                {
                    var creationUser = await eventCacheRepository.GetUserByIdAsync(request.UserId!);
                    await AssignAsync(new AssignmentRequestModel
                    {
                        EventId = response.Id.ToString(),
                        UserId = request.UserId,
                        User = creationUser
                    });
                }
                response.Message = "Event created successfully.";
                return response;
            }
            return new EventResponseModel
            {
                Message = $"An unexpected error occurred. Please, try again."
            };
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            var response = await eventMutationRepository.UpdateEventAsync(request);
            if (response.Success)
            {
                response.Message = "Event updated successfully.";
                return response;
            }
            return new UpdateResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            var response = await eventMutationRepository.DeleteEventAsync(id);
            if (response.Success)
            {
                response.Message = "Event deleted successfully.";
                return response;
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
                return new AssignmentResponseModel
                {
                    Message = "You cannot assign to this event as an administrator, because an event administrator already exists."
                };
            }
            
            var response = await AssignToEventAsync(request);
            if (response.Success)
            {
                response.Message = "Successfully assigned your user to event.";
                return response;
            }
            return new AssignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            var response = await eventMutationRepository.AssignToEventAsync(request);
            if (response.Success)
            {
                var evt = await eventLookupService.GetEventAsync(Guid.Parse(request.EventId!), true);
                var publishingItems = evt.Adapt<EventMessageRequestModel>();
                publishingItems.UserIds = evt!.Users!.Select(u => u.Id.ToString());
                await eventPublishingService.SendEventAsync(publishingItems);
                response.Message = "Successfully assigned user to event.";
                return response;
            }
            return new AssignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            var response = await eventMutationRepository.UnassignFromEventAsync(request);
            if (response.Success)
            {
                var evt = await eventLookupService.GetEventAsync(Guid.Parse(request.EventId!), true);
                var publishingItem = evt.Adapt<EventMessageRequestModel>();
                publishingItem.UserIds = evt!.Users!.Select(u => u.Id.ToString());
                await eventPublishingService.SendEventAsync(publishingItem);
                response.Message = "Successfully unassigned user from event.";
                return response;
            }
            return new UnassignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request)
        {
            request.EventId = await eventSecurityService.UnprotectAsync(request.EventId!);
            var response = await eventMutationRepository.SettleEventAsync(request);
            if (response.Success)
            {   
                await eventCacheRepository.DeleteActiveEventAsync(request.EventId!);
                response.Message = "The event was successfully settled.";
                return response;
            }
            return new SettleEventResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

       

       
    }
}
