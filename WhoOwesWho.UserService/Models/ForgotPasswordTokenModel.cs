namespace WhoOwesWho.UserService.Models
{
    public class ForgotPasswordTokenModel
    {
        public Guid UserId { get; set; }
        public long ExpirationTime { get; set; }
        public string? ForgotPasswordToken { get; set; }
    }
}
