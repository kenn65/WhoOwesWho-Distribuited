using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Validators
{
    public class CreateEventRequestValidator : AbstractValidator<EventRequestModel>
    {
        public CreateEventRequestValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.EventErrorMessages.NameMissing);

            RuleFor(x => x.Location)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.EventErrorMessages.LocationMissing);

            RuleFor(x => x.Currency)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.EventErrorMessages.CurrencyMissing);

            RuleFor(x => x.StartDate)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(Constants.EventErrorMessages.StartDateMissing);
        }
    }
}
