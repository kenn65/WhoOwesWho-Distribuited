using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.AuthorizationService.Models
{
    public class AuthenticationRequestModel 
    {
        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("host")]
        public string? Host { get; set; }
    }
}
