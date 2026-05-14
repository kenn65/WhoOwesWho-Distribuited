using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.Shared.Auxiliaries;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentCommandService
    {
        Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request);
        Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId);
    }

    public class PaymentCommandService(
        IConfiguration configuration,
        IPaymentCalculationService paymentCalculationService,
        IPaymentMutationRepository paymentMutationRepository
        ) : ServiceBase(configuration), IPaymentCommandService
    {
        public async Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            var userIdList = request.UserIds!.ToList();
            var timeTicks = DateTime.Now.Ticks;
            var amountCalculation = await paymentCalculationService.CalculateAmount(request);
            request.Amount = amountCalculation.Amount;
            request.Currency = amountCalculation.Currency;
            request.TotalAmount = amountCalculation.TotalAmount;
            request.PaymentId = Guid.NewGuid();
            request.CreditorIncluded = request.UserIds!.Any(u => u == request.CreditorId.ToString());

            if (request.CreditorIncluded && request.UserIds!.Count() == 1)
            {
                return new CreatePaymentResponseModel
                {
                    Message = Constants.PaymentErrorMessages.PaymentInvalid
                };
            }

            var paymentAddition = await paymentMutationRepository.AddPaymentAsync(request, timeTicks);
            if (!paymentAddition.Success)
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }

            foreach (var userId in request.UserIds!.Where(u => u != request.CreditorId.ToString()))
            {
                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                var creditUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, true);
                if (!creditUserAddition.Success)
                {
                    throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
                }

                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                request.DebitorId = Guid.Parse(userId);
                var debitUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, false);
                if (!debitUserAddition.Success)
                {
                    throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
                }
            }

            return new CreatePaymentResponseModel
            {
                Message = Constants.PaymentErrorMessages.PaymentAdditionSucceeded,
                Success = true
            };
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId)
        {
            var response = await paymentMutationRepository.DeletePaymentAsync(paymentId);
            response.Success = true;
            response.Message = Constants.PaymentErrorMessages.PamentRemovalSucceeded;
            return response;
        }

        public async Task<UpdatePaymentResponseModel> UpdatePaymentDetailsAsync(UpdatePaymentRequestModel request)
        {
            var userIdList = request.UserIds!.ToList();
            request.UserIds = userIdList;

            var timeTicks = DateTime.Now.Ticks;
            var amountCalculation = await paymentCalculationService.CalculateAmount(request);
            request.Amount = amountCalculation.Amount;
            request.Currency = amountCalculation.Currency;
            request.TotalAmount = amountCalculation.TotalAmount;
            request.PaymentId = request.PaymentId;
            request.CreditorIncluded = request.UserIds!.Any(u => u == request.CreditorId.ToString());

            var paymentAddition = await paymentMutationRepository.UpdatePaymentAsync(request);
            if (!paymentAddition.Success)
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }

            var deletePaymentUsersRequest = new DeletePaymentRequestModel
            {
                PaymentId = request.PaymentId.ToString()
            };

            var response = await paymentMutationRepository.DeletePaymentUsersAsync(deletePaymentUsersRequest);
            if (!response.Success)
            {
                throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
            }

            foreach (var userId in request.UserIds!.Where(u => u != request.CreditorId.ToString()))
            {
                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                var creditUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, true);
                if (!creditUserAddition.Success)
                {
                    throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
                }

                timeTicks = new DateTime(timeTicks).AddSeconds(1).Ticks;
                request.DebitorId = Guid.Parse(userId);
                var debitUserAddition = await paymentMutationRepository.AddPaymentUserAsync(request, timeTicks, false);
                if (!debitUserAddition.Success)
                {
                    throw new Exception(Constants.GlobalErrorMessages.UnexpectedError);
                }
            }

            return new UpdatePaymentResponseModel()
            {
                Message = Constants.PaymentErrorMessages.PaymentModificationSucceeded,
                Success = true
            };
        }
    }
}
