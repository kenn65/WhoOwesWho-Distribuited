using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users
{
    public class UserUpdateRequestModel : RequestModelBase
    {
        public string ProtectedId { get; set; } = string.Empty;
        public Guid Id { get; set; } = Guid.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MobilePhoneNumber { get; set; } = string.Empty;
        public bool Admin { get; set; } 
        public string EventId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsPasswordUpdating { get; set; }
    }
}
