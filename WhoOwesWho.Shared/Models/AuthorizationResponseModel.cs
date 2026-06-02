using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.Shared.Models
{
    public class AuthorizationResponseModel : ModelBase
    {
        [JsonProperty("tokenName")]
        public string TokenName => ".WhoOwesWho.Token";

        [JsonProperty("tokenValue")]
        public string? TokenValue { get; set; }

        [JsonProperty("refreshName")]
        public string RefreshName => ".WhoOwesWho.Refresh";

        [JsonProperty("refreshValue")]
        public string RefreshValue { get; set; } = string.Empty;
    }
}
