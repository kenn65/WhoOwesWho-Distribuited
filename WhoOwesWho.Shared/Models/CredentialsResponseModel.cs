using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.AuthorizationService.Models
{
    public class CredentialsResponseModel : ModelBase
    {
        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("fullName")]
        public string? FullName { get; set; }   

        [JsonProperty("password")]
        public string? Password { get; set; }
    }
}
