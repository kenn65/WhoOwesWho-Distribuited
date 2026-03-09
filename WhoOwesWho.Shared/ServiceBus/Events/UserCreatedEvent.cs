namespace WhoOwesWho.Shared.ServiceBus.Events
{
    public record UserCreatedEvent(
        Guid UserId,
        string FullName,
        string Email
    );
}
