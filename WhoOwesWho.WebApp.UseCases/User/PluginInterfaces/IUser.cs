using WhoOwesWho.WebApp.CoreBusiness.Entities.User;

namespace WhoOwesWho.WebApp.UseCases.User.PluginInterfaces
{
    public interface IUser
    {
        Task<UserModel> SignUp(SignUpRequestModel request);
    }
}
