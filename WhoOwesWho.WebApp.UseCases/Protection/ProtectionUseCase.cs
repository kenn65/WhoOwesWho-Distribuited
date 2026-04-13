using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Protection;
using WhoOwesWho.WebApp.UseCases.Protection.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Protection
{
    public interface IProtectionUseCase
    {
        Task<string> ExecuteProtectAsync(string text);
        Task<string> ExecuteUnprotectAsync(string text);
        Task<ProtectionResponseModel> ExecuteProtectCookiesAsync(CookiesRequestModel request);
    }

    public class ProtectionUseCase(IProtection protectionPlugin) : IProtectionUseCase
    {
        public async Task<string> ExecuteProtectAsync(string text)
        {
            var response = await protectionPlugin.ProtectAsync(text);
            return response.ProtectedValue ?? string.Empty;
        }

        public async Task<string> ExecuteUnprotectAsync(string text)
        {
            var response = await protectionPlugin.UnprotectAsync(text);
            return response.UnprotectedValue ?? string.Empty;
        }

        public async Task<ProtectionResponseModel> ExecuteProtectCookiesAsync(CookiesRequestModel request)
        {
            return await protectionPlugin.ProtectCookiesAsync(request);
        }
    }
}
