using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;

namespace WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces
{
    public interface IPaymentsPlugin
    {
        Task<UserHasPaymentsResponseModel> GetUserPaymentsAsync(Guid eventId, Guid userId, bool active, string jwtToken);
        Task<UserBalanceResponseModel> GetUserBalanceAsync(Guid userId, Guid eventId, string jwtToken);
        Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request, string jwtToken);
        Task<PaymentsResponseModel> GetPaymentsDataAsync(Guid eventId, bool active, string jwtToken);
        Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync(Guid paymentId, bool active, string jwtToken);
        Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request, string jwtToken);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId, string jwtToken);

    }
}
