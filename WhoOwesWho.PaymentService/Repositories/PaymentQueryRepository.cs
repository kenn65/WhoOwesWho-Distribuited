using Mapster;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Extensions;
using WhoOwesWho.PaymentService.EfCore.Context;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Repositories
{
    public interface IPaymentQueryRepository
    {
        Task<IEnumerable<UserPaymentResponseModel>> GetUserPaymentsAsync(UserBalanceRequestModel request, bool isCreditor);
        Task<IEnumerable<UserPaymentResponseModel>> GetPaymentsAsync(PaymentsRequestModel request);
        Task<PaymentDetailsModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request);
    }

    public class PaymentQueryRepository(PaymentDbContext context, IPaymentSecurityService paymentSecurityService, IUserGatewayService userGatewayService) : IPaymentQueryRepository
    {

        public async Task<IEnumerable<UserPaymentResponseModel>> GetUserPaymentsAsync(UserBalanceRequestModel request, bool isCreditor)
        {
            var eventId = Guid.Parse(request.EventId!);
            var userId = Guid.Parse(request.UserId!);

            var payments = await context.Payments
                .Join(context.PaymentUsers,
                    p => p.Id,
                    pu => pu.PaymentId,
                    (p, pu) => new { p, pu })
                .Where(x =>
                    x.p.EventId == eventId &&
                    x.pu.UserId == userId &&
                    x.pu.IsCreditor == isCreditor)
                .Select(x => x.p).ProjectToType<UserPaymentResponseModel>()
                .ToListAsync();
            return [.. payments];
        }

        public async Task<IEnumerable<UserPaymentResponseModel>> GetPaymentsAsync(PaymentsRequestModel request)
        {
            var eventId = Guid.Parse(request.EventId!);

            var rows = await context.Payments
                .Join(context.PaymentUsers,
                     p => p.Id,
                     pu => pu.PaymentId,
                     (p, pu) => new { p, pu })
                .Where(x => x.p.EventId == eventId)
                .OrderBy(x => x.pu.Created)
                .Select(x => new
                {
                    x.p.Id,
                    x.p.EventId,
                    x.p.Amount,
                    x.p.Currency,
                    x.p.OriginalAmount,
                    x.p.OriginalCurrency,
                    x.p.Description,
                    x.p.Created,
                    x.pu.UserId,
                    x.pu.IsCreditor
                }).ToListAsync();

            var userPayments = new List<UserPaymentResponseModel>();

            foreach (var row in rows)
            {
                var protectedUserId = await paymentSecurityService.ProtectAsync(row.UserId.ToString());
                var authorizedUser = await userGatewayService.GetAuthorizedUserAsync(protectedUserId, request.Token!, true, false);

                if (row.IsCreditor)
                {
                    userPayments.Add(new UserPaymentResponseModel
                    {
                        Id = row.Id,
                        EventId = row.EventId,
                        Amount = row.Amount,
                        Currency = row.Currency,
                        OriginalAmount = row.OriginalAmount,
                        OriginalCurrency = row.OriginalCurrency,
                        Description = row.Description,
                        Created = new DateTime(row.Created).ToDisplayDateTimeFormat(),
                        CreditEventUser = authorizedUser
                    });
                }
                else
                {
                    var existing = userPayments.LastOrDefault(x => x.Id == row.Id);

                    if (existing != null)
                    {
                        existing.DebitEventUser = authorizedUser;
                    }
                }
            }
            return userPayments;
        }

        public async Task<PaymentDetailsModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
            var paymentId = Guid.Parse(request.PaymentId!);

            var rows = await context.Payments
            .Join(context.PaymentUsers,
                p => p.Id,
                pu => pu.PaymentId,
                (p, pu) => new { p, pu })
            .Where(x => x.p.Id == paymentId)
            .OrderByDescending(x => x.pu.IsCreditor)
            .Select(x => new
            {
                x.p.Id,
                x.p.EventId,
                x.p.Amount,
                x.p.Currency,
                x.p.OriginalAmount,
                x.p.OriginalCurrency,
                x.p.Description,
                x.p.Created,
                x.p.CreditorIncluded,
                x.pu.UserId,
                x.pu.IsCreditor
            })
            .ToListAsync();

            if (rows.Count == 0)
                return new PaymentDetailsModel();

            var userResults = await Task.WhenAll(
                rows.Select(async row =>
                {
                    var protectedUserId = await paymentSecurityService.ProtectAsync(row.UserId.ToString());
                    var user = await userGatewayService.GetAuthorizedUserAsync(protectedUserId, request.Token!, true, false);

                    return new
                    {
                        row,
                        user
                    };
                }));

            var response = new PaymentDetailsModel();
            var debitUsers = new List<UserModel>();

            foreach (var result in userResults)
            {
                var row = result.row;
                var user = result.user;

                if (row.IsCreditor)
                {
                    response.PaymentId = row.Id.ToString();
                    response.EventId = row.EventId.ToString();
                    response.Amount = row.Amount;
                    response.Currency = row.Currency;
                    response.OriginalAmount = row.OriginalAmount;
                    response.OriginalCurrency = row.OriginalCurrency;
                    response.Description = row.Description;
                    response.Created = new DateTime(row.Created).ToDisplayDateTimeFormat();
                    response.CreditorIncluded = row.CreditorIncluded;
                    response.CreditEventUser = user;
                }
                else
                {
                    debitUsers.Add(user);
                }
            }
            response.DebitEventUsers = debitUsers;
            return response;
        }
    }
}
