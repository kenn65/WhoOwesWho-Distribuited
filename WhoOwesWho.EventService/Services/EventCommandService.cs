using Azure.Core;
using Mapster;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Auxiliaries;
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
        Task<SettleEventResponseModel> SettleEventAsync(Guid eventId);
        Task<SettleEventResponseModel> UnsettleEventAsync(Guid eventId);
    }

    public class EventCommandService(IConfiguration configuration,
        IEventLookupService eventLookupService,
        IEventMutationRepository eventMutationRepository,
        IEventCacheRepository eventCacheRepository,
        IEventPublishingService eventPublishingService
        ) : ServiceBase(configuration), IEventCommandService
    {
        public async Task<EventResponseModel?> CreateEventAsync(EventRequestModel request)
        {
            var response = await eventMutationRepository.CreateEventAsync(request);
            if (request.AutoAssign)
            {
                var creationUser = await eventCacheRepository.GetUserByIdAsync(request.UserId.ToString()!);
                await AssignAsync(new AssignmentRequestModel
                {
                    EventId = response!.Id,
                    UserId = request.UserId,
                    User = creationUser
                });
            }
            var evt = await eventLookupService.GetEventAsync(request.Id!, true);
            var publishingItems = evt.Adapt<EventMessageRequestModel>();
            publishingItems!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
            await eventPublishingService.SendEventAsync(publishingItems);
            response!.Message = Constants.EventErrorMessages.EventCreationSucceeded;
            return response;
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            var response = await eventMutationRepository.UpdateEventAsync(request);
            var evt = await eventLookupService.GetEventAsync(request.Id!, true);
            var publishingItems = evt.Adapt<EventMessageRequestModel>();
            publishingItems!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
            await eventPublishingService.SendEventAsync(publishingItems);
            response.Message = Constants.EventErrorMessages.EventModificationSucceeded;
            return response;
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            var response = await eventMutationRepository.DeleteEventAsync(id);
            await eventCacheRepository.DeleteActiveEventAsync(id.ToString());
            response.Message = Constants.EventErrorMessages.EventDeletionSucceeded;
            return response;
        }

        public async Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request)
        {
            var you = await eventCacheRepository.GetUserByIdAsync(request.UserId.ToString());
            var users = (await eventLookupService.GetEventUsersAsync(request.EventId)).ToList();
            if (users.Any(u => u.Admin) && you!.Admin)
            {
                return new AssignmentResponseModel
                {
                    Message = Constants.EventErrorMessages.UserAssignmentInvalid
                };
            }
            var response = await AssignToEventAsync(request);
            response.Message = Constants.EventErrorMessages.UserAssignmentSucceeded;
            return response;
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            var response = await eventMutationRepository.AssignToEventAsync(request);
            var evt = await eventLookupService.GetEventAsync(request.EventId!, true);
            var publishingItems = evt.Adapt<EventMessageRequestModel>();
            publishingItems!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
            await eventPublishingService.SendEventAsync(publishingItems);
            response.Message = Constants.EventErrorMessages.UserAssignmentSucceeded;
            return response;
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            var response = await eventMutationRepository.UnassignFromEventAsync(request);
            var evt = await eventLookupService.GetEventAsync(request.EventId!, true);
            var publishingItem = evt.Adapt<EventMessageRequestModel>();
            publishingItem!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
            await eventPublishingService.SendEventAsync(publishingItem);
            response.Message = Constants.EventErrorMessages.UserUnassignmentSucceeded;
            return response;
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(Guid eventId)
        {
            var response = await eventMutationRepository.SettleEventAsync(eventId);
            var evt = await eventLookupService.GetEventAsync(eventId, false);
            var publishingItem = evt.Adapt<EventMessageRequestModel>();
            publishingItem!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
            await eventPublishingService.SendEventAsync(publishingItem!);
            response.Message = Constants.EventErrorMessages.EventSettlmentSucceeded;
            return response;
        }

        public async Task<SettleEventResponseModel> UnsettleEventAsync(Guid eventId)
        {
            var response = await eventMutationRepository.UnsettleEventAsync(eventId);
            var evt = await eventLookupService.GetEventAsync(eventId, true);
            var publishingItem = evt.Adapt<EventMessageRequestModel>();
            publishingItem!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
            await eventPublishingService.SendEventAsync(publishingItem!);
            response.Message = Constants.EventErrorMessages.EventUnsettlmentSucceeded;
            return response;
        }
    }
}
