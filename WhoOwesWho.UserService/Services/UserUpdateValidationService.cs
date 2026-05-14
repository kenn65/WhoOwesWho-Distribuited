using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserUpdateValidationService
    {
        Task<UpdateUserVerificationModel> ValidateUpdateAsync(UserUpdateRequestModel request);
    }

    public class UserUpdateValidationService(
        IUserCacheRepository userCacheRepository) : IUserUpdateValidationService
    {
        public async Task<UpdateUserVerificationModel> ValidateUpdateAsync(UserUpdateRequestModel request)
        {
            try
            {
                var eventId = await HandleEventIdAsync(request);
                if (eventId == Guid.Empty)
                {
                    return new UpdateUserVerificationModel
                    {
                        Success = true
                    };
                }
                request.EventId = eventId;
                return await HandleAdministratorAsync(request);
            }
            catch
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }
        }

        private async Task<UpdateUserVerificationModel> HandleAdministratorAsync(UserUpdateRequestModel request)
        {
            var activeEvent = await userCacheRepository.GetActiveEventByIdAsync(request.EventId.ToString()!);
            var eventUsers = await GetEventUsersAsync(activeEvent!);
            var existingAdministratorId = eventUsers.FirstOrDefault(u => u.Admin && u.Id != request.Id );
            
            if (request.Admin && existingAdministratorId is not null && existingAdministratorId.Id != request.Id)
            {
                return new UpdateUserVerificationModel
                {
                    Success = false,
                    AdministratorNonExisting = false,
                    Message = Constants.UserUpdatingErrorMessages.AdministratorAlreadyExisting
                };
            }

            if (!request.Admin && existingAdministratorId is not null && existingAdministratorId.Id == request.Id)
            {
                return new UpdateUserVerificationModel
                {
                    Success = false,
                    AdministratorNonExisting = false,
                    Message = Constants.UserUpdatingErrorMessages.NoAdministratorExisting
                };
            }
            return new UpdateUserVerificationModel
            {
                Success = true,
                AdministratorNonExisting = false
            };
        }

        private async Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(EventMessageResponseModel evt)
        {
            return (await Task.WhenAll(
                   evt.UserIds!.Select(async id =>
                   await userCacheRepository.GetUserByIdAsync(id.ToString())
                   ?? new UserMessageResponseModel()
                   ))).ToList();
        }

        private async Task<Guid> HandleEventIdAsync(UserUpdateRequestModel request)
        {
            var eventId = Guid.Empty;
            if (request.EventId == Guid.Empty)
            {
                return Guid.Empty;
            }

            eventId = request.EventId;
            if (request.EventId == Guid.Empty)
            {
                return Guid.Empty;
            }
            return eventId;
        }
    }
}

