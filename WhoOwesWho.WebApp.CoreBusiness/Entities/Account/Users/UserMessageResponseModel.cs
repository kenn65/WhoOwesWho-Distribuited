using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users
{
    public class UserMessageResponseModel : ResponseModelBase
    {

        public Guid Id { get; set; } = Guid.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string MobilePhoneNumber { get; set; } = string.Empty;
        public bool EmailAddressVerified { get; set; }
        public bool Admin { get; set; }
    }
}
