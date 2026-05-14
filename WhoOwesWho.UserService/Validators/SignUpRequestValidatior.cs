using FluentValidation;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Validators
{
    public class SignUpRequestValidatior : AbstractValidator<SignUpRequestModel>
    {
        private readonly AppSettings appSettings;
        public SignUpRequestValidatior(IConfiguration configuration, IUserValidationService userValidationService)
        {
            appSettings = new(configuration);

            RuleFor(x => x.Host)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.GlobalErrorMessages.HostRequired);

            RuleFor(x => x.Entity!.FullName)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .MustAsync((request, fullName, ct) =>
                     userValidationService.IsFullNameUniqueAsync(fullName!))
                 .WithMessage(Constants.CredentialsErrorMessages.FullNameAlreadyExists);

            RuleFor(x => x.Entity!.EmailAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(Constants.CredentialsErrorMessages.EmailAddressInvalid)
                .MustAsync((request, emailAddress, ct) =>
                    userValidationService.IsEmailAddressUniqueAsync(emailAddress!))
                .WithMessage(Constants.CredentialsErrorMessages.EmailAddressAlreadyExists);

            RuleFor(x => x.Entity!.MobilePhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.UserCreationErrorMessages.MobilePhoneNumberRequired);

            RuleFor(x => x.Entity!.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.CredentialsErrorMessages.PasswordMissing)
                .MinimumLength(int.Parse(appSettings.PasswordLengthRequired))
                .WithMessage(GetPasswordRequirementsMessage())
                .Must(ContainUppercase)
                .WithMessage(GetPasswordRequirementsMessage())
                .Must(ContainDigits)
                .WithMessage(GetPasswordRequirementsMessage());
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
