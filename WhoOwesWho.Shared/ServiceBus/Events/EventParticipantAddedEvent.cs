namespace WhoOwesWho.Models.ServiceBus.Events
{
    public record EventUsersAddedEvent(
        Guid EventId,
        Guid UserId
    );
}
