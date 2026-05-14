using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.MessagingService.Validators
{
    public class MessagingRequestValidator : AbstractValidator<MessagingRequestModel>
    {
        public MessagingRequestValidator() 
        {
            RuleFor(x => x.ForgotPasswordToken)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.ForgotPasswordTokenError);

            RuleFor(x => x.User)
                .NotNull()
                .WithMessage(Constants.RequestArgumentErrorMessages.UserIdArgumentError);

            RuleFor(x => x.Host)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.HostArgumentError);

            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.TypeArgumentError);

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(Constants.RequestArgumentErrorMessages.CodeArgumentError);
        }
    }
}
