using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services.Gateways
{
    public interface IUserGatewayService
    {
        public Task<UserModel> GetAuthorizedUserAsync(string userId, string token, bool encode, bool complete = true);
    }

    public class UserGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IUserGatewayService
    {
        public async Task<UserModel> GetAuthorizedUserAsync(string userId, string token, bool encode, bool complete = true)
        {
            var usermodel = await Get<UserModel>(
                $"{AppSettings.UserMicroServiceBaseAddress}/{userId}/{complete}", 
                AppSettings.UserMicroServiceApiKey!,
                encode,
                parameters: null,
                token);
            return await Task.FromResult(usermodel);  
        }
    }

}


