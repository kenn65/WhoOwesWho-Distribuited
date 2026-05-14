using FluentValidation;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Shared.Auxiliaries;

namespace WhoOwesWho.EventService.Validators
{
    public class EventUnassignmentRequestValidator : AbstractValidator<UnassignmentRequestModel>
    {
        public EventUnassignmentRequestValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.EventErrorMessages.EventIdMissing);

            RuleFor(x => x.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.EventErrorMessages.UserIdMissing);
        }
    }
}
