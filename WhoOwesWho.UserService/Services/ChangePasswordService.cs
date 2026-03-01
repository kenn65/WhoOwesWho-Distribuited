using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IChangePasswordService
    {
        Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request);
    }
    public class ChangePasswordService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository
        ) : ServiceBase(configuration), IChangePasswordService
    {
        public async Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request)
        {
              var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress, true);
            if (user is null)
            {
                return await Task.FromResult(new ChangePasswordResponseModel
                {
                    Message = "User not found with the provided email address.",
                    Success = false
                });
            }

           if (user.Password != request.Password)
            {
                return await Task.FromResult(new ChangePasswordResponseModel
                {
                    Message = "The existing password is incorrect.",
                    Success = false
                });
            }

            user.Password = request.NewPassword1;
            //user.Password = await encryptionGatewayService.ProtectAsync(request.NewPassword1!, true);

            var entity = await userMutationRepository.UpdateUserAsync(user);
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
