using Mapster;
using Microsoft.EntityFrameworkCore;
using WhoOwesWho.PaymentService.EfCore.Context;
using WhoOwesWho.PaymentService.EfCore.DataModels;
using WhoOwesWho.PaymentService.Models;

namespace WhoOwesWho.PaymentService.Repositories
{
    public interface IPaymentMutationRepository
    {
        Task<CreatePaymentResponseModel> AddPaymentAsync(CreatePaymentRequestModel request, long timeTicks);
        Task<CreatePaymentResponseModel> AddPaymentUserAsync(CreatePaymentRequestModel request, long timeTicks, bool isCreditor);
        Task<UpdatePaymentResponseModel> UpdatePaymentAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId);
        Task<DeletePaymentResponseModel> DeletePaymentUsersAsync(DeletePaymentRequestModel request);
    }

    public class PaymentMutationRepository(PaymentDbContext context) : IPaymentMutationRepository
    {
        public async Task<CreatePaymentResponseModel> AddPaymentAsync(CreatePaymentRequestModel request, long timeTicks)
        {
            var entity = request.Adapt<Payments>();
            entity.Id = request.PaymentId;
            entity.Created = timeTicks;
            await context.Payments.AddAsync(entity);
            await context.SaveChangesAsync();
            return new CreatePaymentResponseModel
            {
                Success = true,
            };
        }

        public async Task<CreatePaymentResponseModel> AddPaymentUserAsync(CreatePaymentRequestModel request, long timeTicks, bool isCreditor)
        {
            var entity = request.Adapt<PaymentUsers>();
            entity.PaymentId = request.PaymentId;
            entity.Created = timeTicks;
            entity.IsCreditor = isCreditor;
            entity.UserId = isCreditor ? request.CreditorId! : request.DebitorId!;
            await context.PaymentUsers.AddAsync(entity);
            await context.SaveChangesAsync();
            return new CreatePaymentResponseModel
            {
                Success = true
            };
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(Guid paymentId)
        {
            await context.Payments.Where(x => x.Id == paymentId).ExecuteDeleteAsync();
            return new DeletePaymentResponseModel
            {
                Success = true
            };
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentUsersAsync(DeletePaymentRequestModel request)
        {
            var paymentId = Guid.Parse(request.PaymentId!);
            await context.PaymentUsers.Where(x => x.PaymentId == paymentId).ExecuteDeleteAsync();
            return new DeletePaymentResponseModel
            {
                Success = true
            };
        }

        public async Task<UpdatePaymentResponseModel> UpdatePaymentAsync(UpdatePaymentRequestModel request)
        {
            var payment = await context.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId);
            payment!.Amount = request.Amount!.Value;
            payment.TotalAmount = request.TotalAmount;
            payment.OriginalAmount = request.OriginalAmount;
            payment.OriginalCurrency = request.OriginalCurrency;
            payment.Description = request.Description;
            payment.CreditorIncluded = request.CreditorIncluded;
            await context.SaveChangesAsync();
            return new UpdatePaymentResponseModel
            {
                Success = true
            };
        }
    }
}
