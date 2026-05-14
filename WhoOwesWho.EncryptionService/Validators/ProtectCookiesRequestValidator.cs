using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EncryptionService.Validators
{
    public class ProtectCookiesRequestValidator : AbstractValidator<CookiesRequestModel>
    {
        public ProtectCookiesRequestValidator() 
        {
            RuleFor(x => x.User!.Id)
                .NotEmpty()
                .WithMessage(Constants.EventErrorMessages.UserIdMissing);

            RuleFor(x  => x.User!.EmailAddress)
                .NotEmpty()
                .WithMessage(Constants.CredentialsErrorMessages.EmailAddressMissing);
        }
    }
}
