using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Currency;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Event;
using WhoOwesWho.UserService.Services.ServiceBus.Senders.Event;

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
        IPaymentEventMessageSender paymentEventMessageSender,
        IPaymenEventUsersMessageSender paymenEventUsersMessageSender,
        IPaymentCurrenciesMessageSender paymentCurrenciesMessageSender,
        IPaymentExchangeRateMessageSender paymentExchangeRateMessageSender) : ServiceBase(configuration), IPaymentDetailsService
    {
        public async Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
            var paymentDetails = await dataSelectionService.GetPaymentDetailsAsync(request);

            var activeEvent = await paymentEventMessageSender.SendAsync(new SbEventRequestModel
            {
                ApiKey = AppSettings.EventMicroServiceApiKey!,
                UserOrEventId = paymentDetails.EventId!,
                Active = true
            });

            activeEvent.Users = await paymenEventUsersMessageSender.SendAsync(new SbEventRequestModel
            {
                ApiKey = AppSettings.EventMicroServiceApiKey!,
                UserOrEventId = activeEvent.Id.ToString(),
                Active = true
            });
            
            var currencies = await paymentCurrenciesMessageSender.SendAsync(new RequestModelBase
            {
                ApiKey = AppSettings.CurrencyMicroServiceApiKey!
            });
            
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

            var activeEvent = await paymentEventMessageSender.SendAsync(new SbEventRequestModel
            {
                ApiKey = AppSettings.EventMicroServiceApiKey!,
                UserOrEventId = paymentDetails.EventId!,
                Active = false
            });

            activeEvent.Users = await paymenEventUsersMessageSender.SendAsync(new SbEventRequestModel
            {
                ApiKey = AppSettings.EventMicroServiceApiKey!,
                UserOrEventId = activeEvent.Id.ToString(),
                Active = false
            });
            
            var currencies = await paymentCurrenciesMessageSender.SendAsync(new RequestModelBase
            {
                ApiKey = AppSettings.CurrencyMicroServiceApiKey!
            });
            
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
            var activeEventResponse = await paymentEventMessageSender.SendAsync(new SbEventRequestModel
            {
                ApiKey = AppSettings.EventMicroServiceApiKey!,
                UserOrEventId = request.EventId!,
                Active = true
            });

            var exchangeRateResponse = await paymentExchangeRateMessageSender.SendAsync(new ExchangeRateRequestModel
            {
                ApiKey = AppSettings.CurrencyMicroServiceApiKey!,
                PaymentCurrencyIso = request.OriginalCurrency!,
                EventCurrencyIso = activeEventResponse.Currency!
            });

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
