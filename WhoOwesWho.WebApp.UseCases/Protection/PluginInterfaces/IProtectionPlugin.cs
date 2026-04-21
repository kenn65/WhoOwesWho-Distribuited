using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Protection;

namespace WhoOwesWho.WebApp.UseCases.Protection.PluginInterfaces
{
    public interface IProtectionPlugin
    {
        Task<ProtectionResponseModel> ProtectAsync(string text);
        Task<ProtectionResponseModel> UnprotectAsync(string text);
        Task<ProtectionResponseModel> ProtectCookiesAsync(CookiesRequestModel request);
    }
}
