using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserService
    {
        Task<UserModel?> UpdateUserAsync(UserModel request, string token);
    }
    public class UserService(
        IConfiguration configuration, 
        IValidationService validationService, 
        IDataMutationService dataModificationService, 
        IDataQueryService dataSelectionService, 
        IUnprotectValueMessageSender unprotectValueEventService) 
        : ServiceBase(configuration), IUserService
    {
        public async Task<UserModel?> UpdateUserAsync(UserModel request, string token)
        {
            var validationResult = await validationService.VerifyUpdate(request, token);
            if (validationResult is { Success: false, NoAdmin: false })
            {
                return await Task.FromResult(new UserModel()
                {
                    Success = false,
                    Message = "The event you assigned to already has an administrator. If you want to change to being and administrator, the current administrator must uncheck and update before you can check and update."
                });
            }

            request.Id = Guid.Parse(await unprotectValueEventService.SendAsync(request.ProtectedId!));
                        
            var userEntity = await dataSelectionService.GetSingleUserByIdAsync(request.Id, true);
            userEntity!.FullName = request.FullName;
            userEntity.MobilePhoneNumber = request.MobilePhoneNumber;
            userEntity.Admin = request.Admin;
            var response = await Task.FromResult(await dataModificationService.UpdateUserAsync(userEntity));
            if (validationResult is { Success: true, NoAdmin: true })
            {
                return await Task.FromResult(new UserModel()
                {
                    Success = true,
                    Message = "The event running is now left with no administrator. This is indeed not recommended as event and payment edit, delete and settlement are not available as these can only be performed by an administrator."
                });
            }
            return await Task.FromResult(response);
            

        }
    }
}
