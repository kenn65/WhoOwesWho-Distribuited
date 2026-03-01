namespace WhoOwesWho.Models.ServiceBus.Events
{
    public record EventCreatedEvent(
        Guid EventId,
        Guid CreatedByUserId,
        string Name,
        string Currency
    );
}
