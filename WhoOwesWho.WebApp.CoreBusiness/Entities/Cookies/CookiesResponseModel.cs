using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies
{
    public class CookiesResponseModel : ResponseModelBase
    {
        public string TokenName => ".WhoOwesWho.Token";
        public string TokenValue { get; set; } = string.Empty;
        public string RefreshName => ".WhoOwesWho.Refresh";
        public string RefreshValue { get; set; } = string.Empty;
    }
}
