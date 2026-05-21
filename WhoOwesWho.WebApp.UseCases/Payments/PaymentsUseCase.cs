using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Payments
{
    public interface IPaymentsUseCase
    {
        Task<UserHasPaymentsResponseModel> ExecuteAsync(Guid eventId, Guid userId, bool active, string jwtToken);
        Task<UserBalanceResponseModel> ExecuteAsync(Guid userId, Guid eventId, string jwtToken);
        Task<CreatePaymentResponseModel> ExecuteAsync(CreatePaymentRequestModel request, string jwtToken);
        Task<PaymentsResponseModel> ExecuteAsync(Guid eventId, bool active, string jwtToken);
        Task<PaymentDetailsResponseModel> ExecuteAsync(bool active, Guid paymentId, string jwtToken);
        Task<UpdatePaymentResponseModel> ExecuteAsync(UpdatePaymentRequestModel request, string jwtToken);
        Task<DeletePaymentResponseModel> ExecuteAsync(string jwtToken, Guid paymentId);
    }

    public class PaymentsUseCase(IPaymentsPlugin paymentsPlugin) : IPaymentsUseCase
    {
        public async Task<UserHasPaymentsResponseModel> ExecuteAsync(Guid eventId, Guid userId, bool active, string jwtToken)
        {
            return await paymentsPlugin.GetUserPaymentsAsync(eventId, userId, active, jwtToken);
        }

        public async Task<UserBalanceResponseModel> ExecuteAsync(Guid userId, Guid eventId, string jwtToken)
        {
            return await paymentsPlugin.GetUserBalanceAsync(userId, eventId, jwtToken);
        }

        public async Task<CreatePaymentResponseModel> ExecuteAsync(CreatePaymentRequestModel request, string jwtToken)
        {
            return await paymentsPlugin.CreatePaymentAsync(request, jwtToken);
        }

        public async Task<PaymentsResponseModel> ExecuteAsync(Guid eventId, bool active, string jwtToken)
        {
            return await paymentsPlugin.GetPaymentsDataAsync(eventId, active, jwtToken);
        }

        public async Task<PaymentDetailsResponseModel> ExecuteAsync(bool active, Guid paymentId, string jwtToken)
        {
            var payment = await paymentsPlugin.GetPaymentDetailsAsync(paymentId, active, jwtToken);
            IList<string> Ids = [];
            if (payment.PaymentDetails!.CreditorIncluded)
            {
                Ids.Add(payment.PaymentDetails.CreditEventUser!.Id.ToString());
            }
            foreach (var user in payment.PaymentDetails?.DebitEventUsers!)
            {
                Ids.Add(user.Id.ToString());
            }
            payment.PaymentDetails.DebitEventUserIds = Ids;
            return payment;
        }

        public async Task<UpdatePaymentResponseModel> ExecuteAsync(UpdatePaymentRequestModel request, string jwtToken)
        {
            return await paymentsPlugin.UpdatePaymentDetailsAsync(request, jwtToken);
        }

        public async Task<DeletePaymentResponseModel> ExecuteAsync(string jwtToken, Guid paymentId)
        {
            return await paymentsPlugin.DeletePaymentAsync(paymentId, jwtToken);
        }
    }
}
