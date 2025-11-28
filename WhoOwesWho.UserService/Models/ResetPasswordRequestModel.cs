namespace WhoOwesWho.UserService.Models
{
    public class ResetPasswordRequestModel
    {
        public string? EmailAddress { get; set; }
        public string? NewPassword { get; set; }
        public string? NewPasswordRepeat { get; set; }
    }
}
