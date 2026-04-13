using Newtonsoft.Json;

namespace WhoOwesWho.Shared.Models
{
    public class AuthorizationRequestModel
    {
        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }
    }
}
    