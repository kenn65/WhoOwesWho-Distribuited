using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;

namespace WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces
{
    public interface IPaymentsPlugin
    {
        Task<UserPaymentResponseModel> GetUserPaymentsAsync(string eventId, string userId, bool active, string jwtToken);
        Task<UserBalanceResponseModel> GetUserBalanceAsync(string userId, string eventId, string jwtToken);
        Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request, string jwtToken);
    }
}
