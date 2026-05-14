using Newtonsoft.Json;

namespace WhoOwesWho.Shared.Models.Base
{
    public abstract class ModelBase
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("validationErrors")]
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
    }
}

