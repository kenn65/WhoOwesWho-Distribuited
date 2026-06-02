using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class AuthorizationResponseModel : ResponseModelBase
    {
        public static string TokenName => ".WhoOwesWho.Token";
        public string TokenValue { get; set; } = string.Empty;

        public static string RefreshName => ".WhoOwesWho.Refresh";
        public string RefreshValue { get; set; } = string.Empty;

       
    }
}
