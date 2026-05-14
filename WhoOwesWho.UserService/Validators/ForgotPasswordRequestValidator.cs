using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;

namespace WhoOwesWho.UserService.Validators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestModel>
    {
        public ForgotPasswordRequestValidator(IUserValidationService userValidationService)
        {
            RuleFor(x => x.Host)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(Constants.GlobalErrorMessages.HostRequired);

            RuleFor(x => x.EmailAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(Constants.CredentialsErrorMessages.EmailAddressInvalid)
                .MustAsync((request, emailAddress, ct) =>
                    userValidationService.DoesEmailAddressExistAsync(emailAddress))
                .WithMessage(Constants.CredentialsErrorMessages.EmailAddressAlreadyExists);
        }
    }
}
