using Newtonsoft.Json;

namespace WhoOwesWho.UserService.Models
{
    public class ChangePasswordRequestModel
    {
        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("newPassword1")]
        public string? NewPassword1 { get; set; }

        [JsonProperty("newPassword2")]
        public string? NewPassword2 { get; set; }
    }
    
        
    
}
