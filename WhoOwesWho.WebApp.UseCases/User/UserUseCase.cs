using WhoOwesWho.WebApp.CoreBusiness.Entities.User;
using WhoOwesWho.WebApp.UseCases.User.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.User
{
    public interface IUserUseCase
    {
        Task<UserModel> ExecuteAsync(SignUpRequestModel request);
    }

    public class UserUseCase(IUser userPlugin) : IUserUseCase
    {
        public async Task<UserModel> ExecuteAsync(SignUpRequestModel request)
        {
            return await userPlugin.SignUp(request);
        }
    }
}
