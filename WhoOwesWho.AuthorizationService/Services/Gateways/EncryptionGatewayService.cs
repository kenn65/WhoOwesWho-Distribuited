using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services.Gateways
{
    public interface IEncryptionGatewayService
    {
        Task<string> ProtectAsync(string text, bool encode);
        Task<string> UnprotectAsync(string text, bool encode);
        Task<AuthorizationResponseModel> ProtectCookiesAsync(UserModel user, string token, bool encode);
        Task<string> UnprotectCookiesAsync(string cookies);
    }

    public class EncryptionGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IEncryptionGatewayService
    {
        public async Task<string> ProtectAsync(string text, bool encode)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/protect",
                AppSettings.EncryptionMicroServiceApiKey,
                encode,
                new Dictionary<string, dynamic>
                {
                    { "text", text }
                }
            )).ProtectedValue!;
        }

        public async Task<string> UnprotectAsync(string text, bool encode)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/unprotect",
                AppSettings.EncryptionMicroServiceApiKey,
                encode,
                new Dictionary<string, dynamic>
                {
                    { "text", text }
                }
            )).UnprotectedValue!;
        }

        public async Task<AuthorizationResponseModel> ProtectCookiesAsync(UserModel user, string token, bool encode)
        {
            var response = await Post<AuthorizationResponseModel, CookiesRequestModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/cookies/protect",
                new CookiesRequestModel
                {
                    User = user
                },
                AppSettings.EncryptionMicroServiceApiKey,
                encode);

            response.TokenValue = token;
            response.Success = true;
            return await Task.FromResult(response);
        }

        public Task<string> UnprotectCookiesAsync(string cookies)
        {
            throw new NotImplementedException();
        }
    }
}
