using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Services;

namespace WhoOwesWho.UserService.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UserUpdateRequestModel>
    {
        public UpdateUserRequestValidator(IUserValidationService userValdationService) 
        {
             RuleFor(x => x.FullName)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithMessage(Constants.UserCreationErrorMessages.FullNameRequred)
                 .MustAsync((request, fullName, ct) =>
                     userValdationService.IsFullNameUniqueAsync(fullName!))
                 .WithMessage(Constants.CredentialsErrorMessages.FullNameAlreadyExists);
            
            RuleFor(x => x.MobilePhoneNumber)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(Constants.UserCreationErrorMessages.MobilePhoneNumberRequired);
        }
    }
}
