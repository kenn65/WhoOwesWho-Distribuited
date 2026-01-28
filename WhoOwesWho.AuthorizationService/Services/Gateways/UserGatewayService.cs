using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services.Gateways
{
    public interface IUserGatewayService
    {
        public Task<UserModel> GetUserAsync(string emailAddress, bool encode, bool complete = true);
    }

    public class UserGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IUserGatewayService
    {
        public async Task<UserModel> GetUserAsync(string emailAddress, bool encode, bool complete = true)
        {
            return await Get<UserModel>($"{AppSettings.UserMicroServiceBaseAddress}/{emailAddress}",
                AppSettings.UserMicroServiceApiKey,
                encode,
                new Dictionary<string, dynamic>
                {
                   { "complete", complete }
                });
        }
    }


}
