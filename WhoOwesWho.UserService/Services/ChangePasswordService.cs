using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;

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
        IEncryptionGatewayService encryptionGatewayService
        ) : ServiceBase(configuration), IChangePasswordService
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

            var unprotectedExistingPassword = await encryptionGatewayService.UnprotectAsync(user.Password!, true);

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


            user.Password = await encryptionGatewayService.ProtectAsync(request.NewPassword1!, true);

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
