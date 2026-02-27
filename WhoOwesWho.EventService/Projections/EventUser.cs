namespace WhoOwesWho.EventService.Projections
{
    public  class EventUser
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
