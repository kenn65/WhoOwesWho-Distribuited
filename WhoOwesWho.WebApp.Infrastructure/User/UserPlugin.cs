using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.User;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.UseCases.User.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.User
{
    public class UserPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IUser
    {
        private readonly IConfiguration configuration = configuration;

        public async Task<UserModel> SignUp(SignUpRequestModel request)
        {
            var baseAddress = configuration["UserMicroService:BaseAddress"];
            var apiKey = configuration["UserMicroService:Security:ApiKey"];
            var endpoint = $"{baseAddress}/signup";
            return await PutAsync<UserModel, SignUpRequestModel>(endpoint, request, apiKey!, true);
        }
    }
}
