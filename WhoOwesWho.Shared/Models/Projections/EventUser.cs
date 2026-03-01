namespace WhoOwesWho.Models.Models.Projections
{
    public  class EventUser
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = default!;
    }
}
