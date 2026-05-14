using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class SignUpRequestModel : RequestModelBase
    {
        public UserModel? Entity { get; set; }

        public string? Host { get; set; }
    }
}
