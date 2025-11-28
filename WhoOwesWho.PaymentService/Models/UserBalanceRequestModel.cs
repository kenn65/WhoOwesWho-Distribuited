using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class UserBalanceRequestModel
    {
        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
