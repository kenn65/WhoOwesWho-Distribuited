using Newtonsoft.Json;

namespace WhoOwesWho.Shared.Models
{
    public class UserUpdateRequestModel
    {
        [JsonProperty("protectedId")]
        public string? ProtectedId { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.Empty;

        [JsonProperty("fullName")]
        public string? FullName { get; set; }

        [JsonProperty("mobilePhoneNumber")]
        public string? MobilePhoneNumber { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("isPasswordUpdating")]
        public bool IsPasswordUpdating { get; set; }
    }
}
