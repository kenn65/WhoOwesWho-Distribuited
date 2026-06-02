namespace WhoOwesWho.Shared.Models
{
    public class RefreshTokenModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;

        public DateTime ExpiresUtc { get; set; }

        public bool Revoked { get; set; }

        public DateTime CreatedUtc { get; set; }
    }
}
