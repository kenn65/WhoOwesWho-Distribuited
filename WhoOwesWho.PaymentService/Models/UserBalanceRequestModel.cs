using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class UserBalanceRequestModel
    {
        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
    }
}
