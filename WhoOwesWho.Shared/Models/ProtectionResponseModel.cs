using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.Shared.Models
{
    public class ProtectionResponseModel : ModelBase
    {
        public string? ProtectedValue { get; set; }
        public string? UnprotectedValue { get; set; }
    }
}
