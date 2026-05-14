using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.Services.Base;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Services
{
    public interface ICookiesMasterService
    {
        Task SetCookiesAsync(CookiesResponseModel data);
        Task SetAdminCookieAsync(CookiesResponseModel data, bool isAdmin);
        Task DeleteCookiesAsync();
        Task<CookiesResponseModel?> GetAsync();
        Task<bool> IsAuthorizedAsync(CookiesResponseModel cookies);
        Task<bool> IsAdministratorAsync(CookiesResponseModel cookies);
    }

    public class CookiesMasterService(
        IHttpContextAccessor http, 
        IProtectionUseCase protection,
        IJSRuntime js,
        NavigationManager nav
        ) : CookiesServiceBase(js, nav), ICookiesMasterService
    {
        private HttpContext HttpContext => http.HttpContext!;

        public async Task SetCookiesAsync(CookiesResponseModel data)
        {
            await AppendCookiesAsync(data);
        }

        public async Task DeleteCookiesAsync()
        {
            await RemoveCookiesAsync();
        }

        public Task<CookiesResponseModel?> GetAsync()
        {
            var request = HttpContext.Request;

            var data = new CookiesResponseModel();
            var token = request.Cookies[data.TokenName];
            var userId = request.Cookies[data.UserIdName];
            var userEmailAddress = request.Cookies[data.UserEmailAddressName];
            var admin = request.Cookies[data.AdminName];

            if (string.IsNullOrEmpty(token))
                return Task.FromResult<CookiesResponseModel?>(null);

            return Task.FromResult<CookiesResponseModel?>(new CookiesResponseModel
            {
                TokenValue = token,
                UserEmailAddressValue = userEmailAddress!,
                UserIdValue = userId!,
                AdminValue = admin!
            });
        }
        
        public async Task<bool> IsAuthorizedAsync(CookiesResponseModel cookies)
        {
            return !string.IsNullOrEmpty(cookies?.TokenValue);
        }

        public async Task<bool> IsAdministratorAsync(CookiesResponseModel cookies)
        {
            if (string.IsNullOrEmpty(cookies?.AdminValue))
            {
                return false;
            }
            var result = await protection.ExecuteUnprotectAsync(cookies?.AdminValue!);
            return result == "True";
        }

        public async Task SetAdminCookieAsync(CookiesResponseModel data, bool isAdmin)
        {
            var isAdminString = isAdmin ? "True" : "False";
            data.AdminValue = await protection.ExecuteProtectAsync(isAdminString);
            await UpdateAdminCookieAsync(data);
        }
    }
}
