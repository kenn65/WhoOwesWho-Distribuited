namespace WhoOwesWho.EventService.Infrastructure
{
    public class UserSnapshot
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
