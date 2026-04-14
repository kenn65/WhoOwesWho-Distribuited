using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Account
{
    public class UserPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IUser
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<UserModel> SignUp(SignUpRequestModel request)
        {
            var baseAddress = appSettings.UserMicroserviceBaseAddress; //configuration["UserMicroService:BaseAddress"];
            var apiKey = appSettings.UserMicroserviceApiKey; //configuration["UserMicroService:Security:ApiKey"];
            var endpoint = $"{baseAddress}/signup";
            return await PutAsync<UserModel, SignUpRequestModel>(endpoint, request, apiKey!, true);
        }

        public async Task<UserModel> VerifyAsync(VerificationRequestModel request)
        {
            var baseAddress = appSettings.UserMicroserviceBaseAddress; //configuration["UserMicroService:BaseAddress"];
            var apiKey = appSettings.UserMicroserviceApiKey; //configuration["UserMicroService:Security:ApiKey"];
            var endpoint = $"{baseAddress}/emailaddress/verify";
            return await PostAsync<UserModel, VerificationRequestModel>(endpoint, request, apiKey!, true);
        }
    }
}
