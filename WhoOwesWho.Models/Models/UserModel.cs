using Newtonsoft.Json;
using WhoOwesWho.Models.Models.Base;

namespace WhoOwesWho.Models.Models
{
    public class UserModel : ModelBase
    {
        [JsonProperty("protectedId")]
        public string? ProtectedId { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.Empty;

        [JsonProperty("fullName")]
        public string? FullName { get; set; }

        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("mobilePhoneNumber")]
        public string? MobilePhoneNumber { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("emailAddressVerified")]
        public bool EmailAddressVerified { get; set; }

        [JsonIgnore]
        public decimal Balance { get; set; } = 0;
    }
}
