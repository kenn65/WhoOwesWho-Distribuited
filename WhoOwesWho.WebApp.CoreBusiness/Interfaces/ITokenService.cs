using System.Net;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

namespace WhoOwesWho.WebApp.CoreBusiness.Interfaces
{
    public interface ITokenService
    {
        Task SetCookiesAsync(CookiesResponseModel data);
        Task<CookiesDeletionResponseModel> DeleteCookiesAsync();
        Task<CookiesResponseModel?> GetAsync();
        Task<CookiesResponseModel> RefreshAsync();
    }
}
