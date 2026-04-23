using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

namespace WhoOwesWho.WebApp.Services.Base
{
    public abstract class CookiesServiceBase(IJSRuntime js, NavigationManager nav)
    {
        private readonly IJSRuntime _js = js;
        private readonly NavigationManager _nav = nav;
        private string BaseUrl => _nav.BaseUri;

        protected async Task AppendCookiesAsync(CookiesResponseModel data)
        {
            await _js.InvokeVoidAsync(
                "cookieApi.setCookies",
                $"{BaseUrl}api/auth/set-cookies",
                data
            );
        }

        protected async Task UpdateAdminCookieAsync(CookiesResponseModel data)
        {
            await _js.InvokeVoidAsync(
                "cookieApi.setCookies",
                $"{BaseUrl}api/auth/update-admin-cookie",
                data
            );
        }

        protected async Task RemoveCookiesAsync()
        {
            await _js.InvokeVoidAsync(
                "cookieApi.deleteCookies",
                $"{BaseUrl}api/auth/delete-cookies"
            );
        }
    }
}
