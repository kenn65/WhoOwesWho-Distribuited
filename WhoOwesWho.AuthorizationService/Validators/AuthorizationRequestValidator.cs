using FluentValidation;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Validators
{
    public class AuthorizationRequestValidator : AbstractValidator<AuthorizationRequestModel>
    {
        public AuthorizationRequestValidator(IAuthValidationService authValidationService)
        {
            RuleFor(x => x.EmailAddress)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(Constants.CredentialsErrorMessages.EmailAddressMissing)
               .EmailAddress()
               .WithMessage(Constants.CredentialsErrorMessages.EmailAddressInvalid)
               .MustAsync((request, emailAddress, ct) =>
                   authValidationService.DoesEmailExist(emailAddress))
               .WithMessage(Constants.CredentialsErrorMessages.EmailAdddressDoesNotExist);
        }
    }
}
