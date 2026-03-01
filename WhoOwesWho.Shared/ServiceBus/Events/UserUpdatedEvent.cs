namespace WhoOwesWho.Models.ServiceBus.Events
{
    public record UserUpdatedEvent(
        Guid UserId,
        string FullName,
        string Email
    );
}
