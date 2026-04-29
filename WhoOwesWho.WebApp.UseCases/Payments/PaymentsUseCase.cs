using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.UseCases.Payments
{
    public interface IPaymentsUseCase
    {
        Task<UserPaymentResponseModel> ExecuteAsync(string eventId, string userId, bool active, string jwtToken);
        Task<UserBalanceResponseModel> ExecuteAsync(string userId, string eventId, string jwtToken);
        Task<CreatePaymentResponseModel> ExecuteAsync(CreatePaymentRequestModel request, string jwtToken);
    }

    public class PaymentsUseCase(IPaymentsPlugin paymentsPlugin, IProtectionUseCase protectionUseCase) : IPaymentsUseCase
    {
        public async Task<UserPaymentResponseModel> ExecuteAsync(string eventId, string userId, bool active, string jwtToken)
        {
            var protectedEventId = await protectionUseCase.ExecuteProtectAsync(eventId);
            return await paymentsPlugin.GetUserPaymentsAsync(protectedEventId, userId, active, jwtToken);
        }

        public async Task<UserBalanceResponseModel> ExecuteAsync(string userId, string eventId, string jwtToken)
        {
            return await paymentsPlugin.GetUserBalanceAsync(userId, eventId, jwtToken);
        }

        public async Task<CreatePaymentResponseModel> ExecuteAsync(CreatePaymentRequestModel request, string jwtToken)
        {
            return await paymentsPlugin.CreatePaymentAsync(request, jwtToken);
        }
    }
}
