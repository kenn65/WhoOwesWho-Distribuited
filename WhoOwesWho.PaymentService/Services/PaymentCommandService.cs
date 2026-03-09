using Microsoft.Extensions.Configuration.UserSecrets;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentCommandService
    {
        Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request);
        Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(string paymentId);
    }

    public class PaymentCommandService(
        IConfiguration configuration,
        IPaymentCalculationService paymentCalculationService,
        IPaymentMutationRepository paymentMutationRepository
        ) : ServiceBase(configuration), IPaymentCommandService
    {
        public async Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            var timeTicks = DateTime.Now.Ticks;
            var amountCalculation = await paymentCalculationService.CalculateAmount(request);
            request.Amount = amountCalculation.Amount;
            request.Currency = amountCalculation.Currency;
            request.TotalAmount = amountCalculation.TotalAmount;
            request.PaymentId = Guid.NewGuid();
            request.CreditorIncluded = request.UserIds!.Any(u => u == request.CreditorId);

            if (request.CreditorIncluded && request.UserIds!.Count() == 1)
            {
                return await Task.FromResult(new CreatePaymentResponseModel
                {
                    Message = "Payment invalid as the only debtor is yourself, which does not make sense."
                });
            }

            var paymentAddition = await paymentMutationRepository.AddPaymentAsync(request, timeTicks);
            if (!paymentAddition.Success)
            {
                await CreateUnsuccessfulPaymentResponseAsync();
            }
            
            foreach (var userId in request.UserIds!.Where(u => u != request.CreditorId))
            {
                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                var creditUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, true);
                if (!creditUserAddition.Success)
                {
                    return await CreateUnsuccessfulPaymentResponseAsync();
                } 

                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                request.DebitorId = userId;
                var debitUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, false);
                if (!debitUserAddition.Success)
                {
                    return await CreateUnsuccessfulPaymentResponseAsync();
                }
            }
        
            return new CreatePaymentResponseModel
            {
            Message = "Payment added successfully.",
                Success = true
            };
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(string paymentId)
        {
            return await paymentMutationRepository.DeletePaymentAsync(paymentId);
        }

        public async Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request)
        {
            var timeTicks = DateTime.Now.Ticks;
            var amountCalculation = await paymentCalculationService.CalculateAmount(request);
            request.Amount = amountCalculation.Amount;
            request.Currency = amountCalculation.Currency;
            request.TotalAmount = amountCalculation.TotalAmount;
            request.PaymentId = request.PaymentId;
            request.CreditorIncluded = request.UserIds!.Any(u => u == request.CreditorId);

            var paymentAddition = await paymentMutationRepository.UpdatePaymentAsync(request);
            if (!paymentAddition.Success)
            {
                await CreateUnsuccessfulPaymentResponseAsync();
            }

            var deletePaymentUsersRequest = new DeletePaymentRequestModel
            {
                PaymentId = request.PaymentId.ToString()
            };

            var response = await paymentMutationRepository.DeletePaymentUsersAsync(deletePaymentUsersRequest);
            if (!response.Success)
            {
                await CreateUnsuccessfulPaymentResponseAsync();
            }

            foreach (var userId in request.UserIds!.Where(u => u != request.CreditorId))
            {
                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                var creditUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, true);
                if (!creditUserAddition.Success)
                {
                    await CreateUnsuccessfulPaymentResponseAsync();
                }

                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                request.DebitorId = userId;
                var debitUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, false);
                if (!debitUserAddition.Success)
                {
                    await CreateUnsuccessfulPaymentResponseAsync();
                }
            }

            return await Task.FromResult(new UpdatePaymentResponseModel()
            {
                Message = "Payment updated successfully.",
                Success = true
            });
        }

        private static async Task<CreatePaymentResponseModel> CreateUnsuccessfulPaymentResponseAsync()
        {
            return new CreatePaymentResponseModel
            {
                Success = false,
                Message = "An unexpected error occurred. Please, try again."
            };
        }


    }
}
