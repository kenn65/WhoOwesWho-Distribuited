namespace WhoOwesWho.Shared.Models.Base.ServiceBus
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


        public record UserDispatchEvent(
        string? Type,
        Guid UserId,
        string? Email,
        DateTimeOffset CreatedAt);

        public record UserDispatchFailedEvent(
            string? Type,
            Guid UserId,
            string? Email,
            string Reason,
            DateTimeOffset FailedAt);

        public record PaymentDispatchEvent(
            string? Type,
            Guid PaymentId,
            string? Name,
            DateTimeOffset CreatedAt);

        public record PaymentDispatchFailedEvent(
            string? Type,
            Guid PaymentId,
            string? Name,
            string Reason,
            DateTimeOffset FailedAt);
    }
}
