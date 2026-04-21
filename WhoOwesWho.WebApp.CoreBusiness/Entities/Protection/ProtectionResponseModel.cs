using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Protection
{
    public class ProtectionResponseModel : ResponseModelBase
    {
        public string ProtectedValue { get; set; } = string.Empty;
        public string UnprotectedValue { get; set; } = string.Empty;
    }
}
