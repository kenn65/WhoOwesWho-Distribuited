namespace WhoOwesWho.Models.Models.Base.ServiceBus
{
    public class ObservabilityRecords
    {
        public record MessagingDispatchedEvent(
            string? Type,
            string? UserEmail,
            DateTimeOffset DispatchedAt);

        public record MessagingDispatchFailedEvent(
            string? Type,
            string? UserEmail,
            string Reason,
            DateTimeOffset FailedAt);

    }
}
