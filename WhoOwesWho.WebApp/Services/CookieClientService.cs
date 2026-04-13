namespace WhoOwesWho.WebApp.Services
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

    public interface ICookieClientService
    {
        Task SetCookiesAsync(CookiesResponseModel data);
        Task<CookiesResponseModel?> GetCookiesAsync();
        Task SignOutAsync();
        Task<bool> IsAuthorizedAsync();
        Task<bool> IsAdministratorAsync();
        Task GoHomeAsync();
    }

    public class CookieClientService(IJSRuntime js, NavigationManager nav) : ICookieClientService
    {
        private readonly IJSRuntime _js = js;
        private readonly NavigationManager _nav = nav;

        private string BaseUrl => _nav.BaseUri;

        // SET COOKIES
        public async Task SetCookiesAsync(CookiesResponseModel data)
        {
            await _js.InvokeVoidAsync(
                "cookieApi.setCookies",
                $"{BaseUrl}api/auth/set-cookies",
                data
            );
        }

        // GET COOKIES
        public async Task<CookiesResponseModel?> GetCookiesAsync()
        {
            return await _js.InvokeAsync<CookiesResponseModel>(
                "cookieApi.getCookies",
                $"{BaseUrl}api/auth/get-cookies"
            );
        }

        // DELETE COOKIES
        public async Task SignOutAsync()
        {
            await _js.InvokeVoidAsync(
                "cookieApi.deleteCookies",
                $"{BaseUrl}api/auth/delete-cookies"
            );
        }

        // IS AUTHORIZED
        public async Task<bool> IsAuthorizedAsync()
        {
            var cookies = await GetCookiesAsync();
            return !string.IsNullOrEmpty(cookies?.TokenValue);
        }

        public async Task<bool> IsAdministratorAsync()
        {
            var cookies = await GetCookiesAsync();
            return !string.IsNullOrEmpty(cookies?.AdminValue) && cookies.AdminValue == "true";
        }

        // GO HOME
        public async Task GoHomeAsync()
        {
            if (await IsAuthorizedAsync())
                _nav.NavigateTo("/me", forceLoad: true);
            else
                _nav.NavigateTo("/", forceLoad: true);
        }
    }        
}
