using FluentValidation;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Shared.Auxiliaries;

namespace WhoOwesWho.EventService.Validators
{
    public class EventAssignmentRequestValidator : AbstractValidator<AssignmentRequestModel>
    {
        public EventAssignmentRequestValidator()
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
