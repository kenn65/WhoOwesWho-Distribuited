using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestModel>
    {
        private readonly AppSettings appSettings;

        public ResetPasswordRequestValidator(IConfiguration configuration, IUserValidationService userValidationService)
        {
            appSettings = new(configuration);

            RuleFor(x => x.EmailAddress)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .EmailAddress()
               .WithMessage(Constants.CredentialsErrorMessages.EmailAddressInvalid)
               .MustAsync((request, emailAddress, ct) =>
                   userValidationService.DoesEmailAddressExistAsync(emailAddress!))
               .WithMessage(Constants.CredentialsErrorMessages.EmailAddressAlreadyExists);

            RuleFor(x => x.NewPassword)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithMessage(Constants.CredentialsErrorMessages.PasswordMissing)
                 .MinimumLength(int.Parse(appSettings.PasswordLengthRequired))
                 .WithMessage(GetPasswordRequirementsMessage())
                 .Must(ContainUppercase)
                 .WithMessage(GetPasswordRequirementsMessage())
                 .Must(ContainDigits)
                 .WithMessage(GetPasswordRequirementsMessage());

            RuleFor(x => x.NewPasswordRepeat)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithMessage(Constants.CredentialsErrorMessages.PasswordMissing)
                 .MinimumLength(int.Parse(appSettings.PasswordLengthRequired))
                 .WithMessage(GetPasswordRequirementsMessage())
                 .Must(ContainUppercase)
                 .WithMessage(GetPasswordRequirementsMessage())
                 .Must(ContainDigits)
                 .WithMessage(GetPasswordRequirementsMessage())
                 .Equal(x => x.NewPassword)
                 .WithMessage(Constants.ResetPasswordErrorMessages.PasswordsDoNotMatch);
        }

        private bool ContainUppercase(string password)
        {
            return password.Count(char.IsUpper)
                >= int.Parse(appSettings.PasswordUppercaseRequired);
        }

        private bool ContainDigits(string password)
        {
            return password.Count(char.IsDigit)
                >= int.Parse(appSettings.PasswordDigitsRequired);
        }

        private string GetPasswordRequirementsMessage()
        {
            return string.Format(
                Constants.CredentialsErrorMessages.PasswordRequirements,
                appSettings.PasswordLengthRequired,
                appSettings.PasswordUppercaseRequired,
                appSettings.PasswordDigitsRequired);
        }
    }
}
