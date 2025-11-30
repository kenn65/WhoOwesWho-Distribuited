using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IChangePasswordService
    {
        Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request);
    }
    public class ChangePasswordService(
        IConfiguration configuration,
        IDataMutationService dataModificationService,
        IDataQueryService dataSelectionService,
        IProtectValueMessageSender protectValueMessageSender,
        IUnprotectValueMessageSender unprotectValueEventService)
        : ServiceBase(configuration), IChangePasswordService
    {
        public async Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request)
        {

            var user = await dataSelectionService.GetSingleUserByEmailAddressAsync(request.EmailAddress, true);
            if (user == null)
            {
                return await Task.FromResult(new ChangePasswordResponseModel
                {
                    Message = "User not found with the provided email address.",
                    Success = false
                });
            }

            var unprotectedExistingPassword = await unprotectValueEventService.SendAsync(new UnprotectValueRequestModel
            {
                ApiKey = AppSettings.EncryptionMicroServiceApiKey,
                Text = user.Password!
            });
                        
            if (unprotectedExistingPassword != request.Password)
            {
                return await Task.FromResult(new ChangePasswordResponseModel
                {
                    Message = "The existing password is incorrect.",
                    Success = false
                });
            }

            if (request.NewPassword1 != request.NewPassword2)
            {
                return await Task.FromResult(new ChangePasswordResponseModel
                {
                    Message = "The new passwords do not match.",
                    Success = false
                });
            }


            user.Password = await protectValueMessageSender.SendAsync(new ProtectValueRequestModel
            {
                ApiKey = AppSettings.EncryptionMicroServiceApiKey,
                Text = request.NewPassword1!
            });
            
            var entity = await dataModificationService.UpdateUserAsync(user);
            if (entity != null)
            {
                return await Task.FromResult(new ChangePasswordResponseModel
                {
                    Message = "Password changed successfully.",
                    Success = true
                });
            }

            return await Task.FromResult(new ChangePasswordResponseModel
            {
                Message = "An error occurred while updating the user password.",
                Success = false
            });


        }
    }
}
