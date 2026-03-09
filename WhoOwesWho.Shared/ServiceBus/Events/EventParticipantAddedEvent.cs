namespace WhoOwesWho.Shared.ServiceBus.Events
{
    public record EventUsersAddedEvent(
        Guid EventId,
        Guid UserId
    );
}
