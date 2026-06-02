namespace WhoOwesWho.EncryptionService.Models
{
    public class DecryptedCookiesModel
    {
        public string TokenName => ".WhoOwesWho.Token";
        public string? TokenValue { get; set; }

        //public string UserIdName => ".WhoOwesWho.UserId";
        //public Guid UserIdValue { get; set; }
        
        //public string UserEmailAddressName => ".WhoOwesWho.Email";
        //public string? UserEmailAddressValue { get; set; }

        //public string AdminName => ".WhoOwesWho.UserAdmin";
        //public bool AdminValue { get; set; }
    }
}
