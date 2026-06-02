using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;

namespace WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces
{
    public interface IPaymentsPlugin
    {
        Task<UserHasPaymentsResponseModel> GetUserPaymentsAsync(Guid eventId, Guid userId, bool active);
        Task<UserBalanceResponseModel> GetUserBalanceAsync(Guid userId, Guid eventId);
        Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request);
        Task<PaymentsResponseModel> GetPaymentsDataAsync(Guid eventId, bool active);
        Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync(Guid paymentId, bool active);
        Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId);

    }
}
