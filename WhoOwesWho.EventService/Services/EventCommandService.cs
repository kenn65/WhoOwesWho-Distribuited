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
            try
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
                response!.Message = Constants.EventErrorMessages.EventCreationSucceeded;
                return response;
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            try
            {
                var response = await eventMutationRepository.UpdateEventAsync(request);
                response.Message = Constants.EventErrorMessages.EventModificationSucceeded;
                return response;
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            try
            {
                var response = await eventMutationRepository.DeleteEventAsync(id);
                response.Message = Constants.EventErrorMessages.EventDeletionSucceeded;
                return response;
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        public async Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request)
        {
            try
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
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            try
            {
                var response = await eventMutationRepository.AssignToEventAsync(request);
                var evt = await eventLookupService.GetEventAsync(request.EventId!, true);
                var publishingItems = evt.Adapt<EventMessageRequestModel>();
                publishingItems!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
                await eventPublishingService.SendEventAsync(publishingItems);
                response.Message = Constants.EventErrorMessages.UserAssignmentSucceeded;
                return response;
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            try
            {
                var response = await eventMutationRepository.UnassignFromEventAsync(request);
                var evt = await eventLookupService.GetEventAsync(request.EventId!, true);
                var publishingItem = evt.Adapt<EventMessageRequestModel>();
                publishingItem!.UserIds = evt!.Users!.Select(u => u.Id.ToString());
                await eventPublishingService.SendEventAsync(publishingItem);
                response.Message = Constants.EventErrorMessages.UserUnassignmentSucceeded;
                return response;
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(Guid eventId)
        {
            try
            {
                var response = await eventMutationRepository.SettleEventAsync(eventId);
                await eventCacheRepository.DeleteActiveEventAsync(eventId.ToString());
                response.Message = Constants.EventErrorMessages.EventSettlmentSucceeded;
                return response;
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }

        }
    }
}
