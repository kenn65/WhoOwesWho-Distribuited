using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base.ServiceBus;

namespace WhoOwesWho.Shared.Models
{
    public class UserMessageRequestModel : RequestModelBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.Empty;

        [JsonProperty("fullName")]
        public string? FullName { get; set; }

        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; } 

        [JsonProperty("mobilePhoneNumber")]
        public string? MobilePhoneNumber { get; set; }

        [JsonProperty("emailAddressVerified")]
        public bool EmailAddressVerified { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        public static string? Type => "user";
    }
}
