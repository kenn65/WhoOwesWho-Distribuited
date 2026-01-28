using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentDetailsService
    {
        Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request);
        Task<PaymentDetailsPageResponseModel> GetSettlementDetailsAsync(SettlementDetailsRequestModel request);
        Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(DeletePaymentRequestModel request);
    }

    public class PaymentDetailsService(
        IConfiguration configuration,
        IDataQueryService dataSelectionService,
        IDataMutationService dataModificationService,
        IEventGatewayService eventGatewayService,
        ICurrencyGatewayService currencyGatewayService
        ) : ServiceBase(configuration), IPaymentDetailsService
    {
        public async Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
            var paymentDetails = await dataSelectionService.GetPaymentDetailsAsync(request);
            var activeEvent = await eventGatewayService.GetEventAsync(paymentDetails.EventId!, request.Token!, true, true);
            activeEvent.Users = await eventGatewayService.GetEventUsersAsync(activeEvent.Id.ToString(), request.Token!, true, true);
            var currencies = await currencyGatewayService.GetCurrenciesAsync(request.Token!);

            return await Task.FromResult(new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = activeEvent,
                Currencies = currencies
            });
        }

        public async Task<PaymentDetailsPageResponseModel> GetSettlementDetailsAsync(SettlementDetailsRequestModel request)
        {
            var paymentDetails = await dataSelectionService.GetPaymentDetailsAsync(request);
            var activeEvent = await eventGatewayService.GetEventAsync(paymentDetails.EventId!, request.Token!, true, false);
            activeEvent.Users = await eventGatewayService.GetEventUsersAsync(activeEvent.Id.ToString(), request.Token!, true, false);
            var currencies = await currencyGatewayService.GetCurrenciesAsync(request.Token!);

            return await Task.FromResult(new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = activeEvent,
                Currencies = currencies
            });
        }
        public async Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request)
        {
            var timeTicks = DateTime.Now.Ticks;
            var amountCalculation = await CalculateAmount(request);
            request.Amount = amountCalculation.Amount;
            request.Currency = amountCalculation.Currency;
            request.TotalAmount = amountCalculation.TotalAmount;
            request.PaymentId = request.PaymentId;
            request.CreditorIncluded = request.UserIds!.Any(u => u == request.CreditorId);

            var paymentAddition = await dataModificationService.UpdatePaymentAsync(request);
            if (!paymentAddition.Success)
            {
                await CreateUnsuccessfulPaymentResponseAsync();
            }

            var deletePaymentUsersRequest = new DeletePaymentRequestModel
            {
                PaymentId = request.PaymentId.ToString()
            };

            var response = await dataModificationService.DeletePaymentUsersAsync(deletePaymentUsersRequest);
            if (!response.Success)
            {
                await CreateUnsuccessfulPaymentResponseAsync();
            }

            foreach (var userId in request.UserIds!.Where(u => u != request.CreditorId))
            {
                timeTicks = new DateTime(timeTicks).AddMicroseconds(100).Ticks;
                var creditUserAddition = await dataModificationService.AddPaymentUserAsync(request, timeTicks, true);
                if (!creditUserAddition.Success)
                {
                    return await CreateUnsuccessfulPaymentResponseAsync();
                }

                timeTicks = new DateTime(timeTicks).AddMicroseconds(100).Ticks;
                request.DebitorId = userId;
                var debitUserAddition = await dataModificationService.AddPaymentUserAsync(request, timeTicks, false);
                if (!debitUserAddition.Success)
                {
                    return await CreateUnsuccessfulPaymentResponseAsync();
                }
            }

            return await Task.FromResult(new UpdatePaymentResponseModel()
            {
                Message = "Payment updated successfully.",
                Success = true
            });
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(DeletePaymentRequestModel request)
        {
            return await Task.FromResult(await dataModificationService.DeletePaymentAsync(request));
        }

        private async Task<CalculateAmountResponseModel> CalculateAmount(UpdatePaymentRequestModel request)
        {
            var activeEventResponse = await eventGatewayService.GetEventAsync(request.EventId!, request.Token!, true, true);
            var exchangeRateResponse = await currencyGatewayService.GetExchangeRateAsync(request.OriginalCurrency!,
                activeEventResponse.Currency!, request.Token!);
            var usersCount = request.UserIds!.Count();
            var totalAmount = request.OriginalAmount * exchangeRateResponse.ExchangeRate;
            return await Task.FromResult(new CalculateAmountResponseModel
            {
                TotalAmount = totalAmount,
                Amount = totalAmount / usersCount,
                Currency = activeEventResponse.Currency

            });
        }
        private static async Task<UpdatePaymentResponseModel> CreateUnsuccessfulPaymentResponseAsync()
        {
            return await Task.FromResult(new UpdatePaymentResponseModel
            {
                Success = false,
                Message = "An unexpected error occurred. Please, try again."
            });
        }
    }
}
