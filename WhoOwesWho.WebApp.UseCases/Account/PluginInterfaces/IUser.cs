using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;

namespace WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces
{
    public interface IUser
    {
        Task<UserModel> SignUp(SignUpRequestModel request);
        Task<UserModel> VerifyAsync(VerificationRequestModel request);
    }
}
