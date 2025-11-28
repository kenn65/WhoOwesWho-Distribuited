using Newtonsoft.Json;

namespace WhoOwesWho.UserService.Models
{
    public class UpdateUserRequestModel
    {
        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("fullName")] 
        public string? FullName { get; set; }

        [JsonProperty("mobilePhoneNumber")]
        public string? MobilePhoneNumber { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
