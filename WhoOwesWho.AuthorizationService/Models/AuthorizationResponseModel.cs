using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.AuthorizationService.Models
{
    public class AuthorizationResponseModel : ModelBase    
    {   
        [JsonProperty("tokenName")]
        public string TokenName => ".WhoOwesWho.Token";

        [JsonProperty("tokenValue")]
        public string? TokenValue { get; set; }

        [JsonProperty("userIdName")]
        public string UserIdName => ".WhoOwesWho.UserId";

        [JsonProperty("userIdValue")]
        public string? UserIdValue { get; set; }

        [JsonProperty("userEmailAddressName")]
        public string UserEmailAddressName => ".WhoOwesWho.Email";

        [JsonProperty("userEmailAddressValue")]
        public string? UserEmailAddressValue { get; set; }

        [JsonProperty("adminName")]
        public string AdminName => ".WhoOwesWho.UserAdmin";

        [JsonProperty("adminValue")]
        public string? AdminValue { get; set; }
    }
}
