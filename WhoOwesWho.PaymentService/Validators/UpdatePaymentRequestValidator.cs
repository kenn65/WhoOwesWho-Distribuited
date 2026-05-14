using FluentValidation;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Auxiliaries;

namespace WhoOwesWho.PaymentService.Validators
{
    public class UpdatePaymentRequestValidator : AbstractValidator<UpdatePaymentRequestModel>
    {
        public UpdatePaymentRequestValidator()
        {
            RuleFor(x => x.PaymentId)
                 .NotEmpty()
                 .WithMessage(Constants.RequestArgumentErrorMessages.PaymentIdArgumentError);

            RuleFor(x => x.EventId)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.EventIdArgumentError);

            RuleFor(x => x.TotalAmount)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.TotalAmountArgumentError);

            RuleFor(x => x.CreditorId)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.CreditorIdArgumentError);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.CurrencyArgumentEroor);

            RuleFor(x => x.OriginalCurrency)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.OriginalCurrencyArgumentError);

            RuleFor(x => x.OriginalAmount)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.OriginalAmountArgumentError);

            RuleFor(x => x.UserIds)
                .Must(x => x != null && x.Count() >= 2)
                .WithMessage(Constants.RequestArgumentErrorMessages.UserIdsArgumentError);
        }
    }
}
