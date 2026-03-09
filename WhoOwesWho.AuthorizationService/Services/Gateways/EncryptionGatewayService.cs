using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services.Gateways
{
    public interface IEncryptionGatewayService
    {
        Task<string> ProtectAsync(string text, bool encode);
        Task<string> UnprotectAsync(string text, bool encode);
        Task<AuthorizationResponseModel> ProtectCookiesAsync(UserMessageResponseModel user, string token, bool encode);
        Task<string> UnprotectCookiesAsync(string cookies);
    }

    public class EncryptionGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), IEncryptionGatewayService
    {
        public async Task<string> ProtectAsync(string text, bool encode)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/protect/{text}",
                AppSettings.EncryptionMicroServiceApiKey,
                encode,
                parameters: null
            )).ProtectedValue!;
        }

        public async Task<string> UnprotectAsync(string text, bool encode)
        {
            return (await Get<ProtectionResponseModel>(
                $"{AppSettings.EncryptionMicroServiceBaseAddress}/unprotect/{text}",
                AppSettings.EncryptionMicroServiceApiKey,
                encode,
                parameters: null
            )).UnprotectedValue!;
        }

        public async Task<AuthorizationResponseModel> ProtectCookiesAsync(UserMessageResponseModel user, string token, bool encode)
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
