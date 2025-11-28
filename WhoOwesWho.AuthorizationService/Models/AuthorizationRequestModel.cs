using Newtonsoft.Json;

namespace WhoOwesWho.AuthorizationService.Models
{
    public class AuthorizationRequestModel
    {
        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }
    }
}
    