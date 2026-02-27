using Microsoft.Azure.Amqp.Framing;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventService
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, string token, bool active = true);
        Task<EventResponseModel?> GetEventByUserAsync(string userId, string token, bool active = true);
        Task<IEnumerable<EventResponseModel>> GetEventsAsync(string token, bool active = true);
        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, string token, bool active = true);
        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool active = true);
        Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request);
        Task<EventResponseModel?> CreateEventAsync(EventRequestModel request);
        Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request);
        Task<DeleteEventResponseModel> DeleteEventAsync(Guid id);
        Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request);
        Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request);
        Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request);
    }

    public class EventService(
        IConfiguration configuration,
        IEventQueryRepository eventQueryRepository,
        IEventMutationRepository eventMutationRepository,
        IUserGatewayService userGatewayService
        ) : ServiceBase(configuration), IEventService
    {
        public async Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request)
        {
            var you = await userGatewayService.GetAuthorizedUserAsync(request.UserId!, request.Token!, true, false);

            var users = (await GetEventUsersAsync(request.EventId!, request.Token!)).ToList();
            if (users.Any(u => u.Admin) && you.Admin)
            {
                return await Task.FromResult(new AssignmentResponseModel
                {
                    Message = "You cannot assign to this event as an administrator, because an event administrator already exists."
                });
            }

            var result = await eventMutationRepository.AssignToEventAsync(request);
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
                result.Message = "Successfully assigned user to event.";
                return result;
            }
            return new AssignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };
        }

        public async Task<EventResponseModel?> CreateEventAsync(EventRequestModel request)
        {
            var result = await eventMutationRepository.CreateEventAsync(request);
            if (result!.Success)
            {
                result.Message = "Event created successfully.";
                return result;
            }
            return new EventResponseModel
            {
                Message = $"An unexpected error occurred. Please, try again."
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

        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, string token, bool active = true)
        {
            return await eventQueryRepository.GetAssignmentAsync(protectedUserId, token, active);
        }

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

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, string token, bool active = true)
        {
            return await eventQueryRepository.GetEventUsersAsync(eventId, token, active);
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request)
        {
            var result = await eventMutationRepository.SettleEventAsync(request);
            if (result.Success)
            {
                result.Message = "The event was successfully settled.";
                return result;
            }
            return new SettleEventResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
            };  
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            var result = await eventMutationRepository.UnassignFromEventAsync(request);
            if (result.Success)
            {
                result.Message = "Successfully unassigned user from event.";
                return result;
            } 
            return new UnassignmentResponseModel
            {
                Message = "An unexpected error occurred. Please, try again."
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
    }
}
