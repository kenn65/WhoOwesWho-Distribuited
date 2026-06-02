using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Payments
{
    public interface IPaymentsUseCase
    {
        Task<UserHasPaymentsResponseModel> ExecuteAsync(Guid eventId, Guid userId, bool active);
        Task<UserBalanceResponseModel> ExecuteAsync(Guid userId, Guid eventId);
        Task<CreatePaymentResponseModel> ExecuteAsync(CreatePaymentRequestModel request);
        Task<PaymentsResponseModel> ExecuteAsync(Guid eventId, bool active);
        Task<PaymentDetailsResponseModel> ExecuteAsync(bool active, Guid paymentId);
        Task<UpdatePaymentResponseModel> ExecuteAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> ExecuteAsync(Guid paymentId);
    }

    public class PaymentsUseCase(IPaymentsPlugin paymentsPlugin) : IPaymentsUseCase
    {
        public async Task<UserHasPaymentsResponseModel> ExecuteAsync(Guid eventId, Guid userId, bool active)
        {
            return await paymentsPlugin.GetUserPaymentsAsync(eventId, userId, active);
        }

        public async Task<UserBalanceResponseModel> ExecuteAsync(Guid userId, Guid eventId)
        {
            return await paymentsPlugin.GetUserBalanceAsync(userId, eventId);
        }

        public async Task<CreatePaymentResponseModel> ExecuteAsync(CreatePaymentRequestModel request)
        {
            return await paymentsPlugin.CreatePaymentAsync(request);
        }

        public async Task<PaymentsResponseModel> ExecuteAsync(Guid eventId, bool active)
        {
            return await paymentsPlugin.GetPaymentsDataAsync(eventId, active);
        }

        public async Task<PaymentDetailsResponseModel> ExecuteAsync(bool active, Guid paymentId)
        {
            var payment = await paymentsPlugin.GetPaymentDetailsAsync(paymentId, active);
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

        public async Task<UpdatePaymentResponseModel> ExecuteAsync(UpdatePaymentRequestModel request)
        {
            return await paymentsPlugin.UpdatePaymentDetailsAsync(request);
        }

        public async Task<DeletePaymentResponseModel> ExecuteAsync(Guid paymentId)
        {
            return await paymentsPlugin.DeletePaymentAsync(paymentId);
        }
    }
}
