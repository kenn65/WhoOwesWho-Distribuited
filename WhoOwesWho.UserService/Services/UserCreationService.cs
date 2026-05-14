using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserCreationService
    {
        Task<UserModel?> CreateUserAsync(SignUpRequestModel request);
    }

    public class UserCreationService(
        IConfiguration configuration,
        IUserCommandService userCommandService
        ) : ServiceBase(configuration), IUserCreationService
    {
        public async Task<UserModel?> CreateUserAsync(SignUpRequestModel request)
        {
            var response = new UserModel();
            var check = await userCommandService.CreateUserAsync(request.Entity!, request.Host!) is not null;
            response.Success = check;
            response.Message = !check
                ? throw new Exception(Constants.GlobalErrorMessages.UnexpectedError)
                : Constants.UserCreationErrorMessages.SignupSucceeded;
            
            return response;
        }
    }
}
