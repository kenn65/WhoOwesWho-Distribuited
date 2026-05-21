using FluentValidation;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Settings;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Validators
{
    public class AuthenticationRequestValidatior : AbstractValidator<AuthenticationRequestModel>
    {
        private readonly AppSettings appSettings;
        public AuthenticationRequestValidatior(IConfiguration configuration, IAuthValidationService authValidationService)
        {
            appSettings = new(configuration);

            RuleFor(x => x.Host)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.GlobalErrorMessages.HostRequired);

            RuleFor(x => x.EmailAddress)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(Constants.CredentialsErrorMessages.EmailAddressMissing)
               .EmailAddress()
               .WithMessage(Constants.CredentialsErrorMessages.EmailAddressInvalid)
               .MustAsync((request, emailAddress, ct) =>
                   authValidationService.DoesEmailExist(emailAddress))
               .WithMessage(Constants.AuthenticationErrorMessages.CredentialsInvalid);

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.CredentialsErrorMessages.PasswordMissing)
                .MustAsync((request, password, ct) =>
                    authValidationService.IsPasswordValid(
                    request.EmailAddress!,
                    password!))
                .WithMessage(Constants.AuthenticationErrorMessages.CredentialsInvalid)
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
