using AutoFixture;
using Newtonsoft.Json.Serialization;
using System.Data.SqlTypes;

namespace WhoOwesWho.Shared.Auxiliaries
{
    public struct Constants
    {
        public struct DateTimeFormats
        {
            public const string IsoDateTimeFormat = "yyyy-MM-ddThh:mm:ss";
            public const string DisplayDateTimeFormat = "dd-MM-yyyy hh:mm:ss";
            public const string DisplayDateFormat = "dd-MM-yyyy";
            public const string IsoDateFormat = "yyyy-MM-dd";
        }

        public struct SessionKeys
        {
            public const string SessionAlertKey = "WhoOwesWhoAlertKey";
        }

        public struct AuthenticationErrorMessages
        {
            public const string CredentialsNotProvided = "E-mail address or password was not provided";
            public const string CredentialsInvalid = "Invalid e-mail address or password.";
            public const string UserWithEmailAddress = "User with e-mail address:";
            public const string WasNotFound = "was not found";
            public const string EmailAddress = "E-mail address:";
            public const string NotVerified = "Your account is not verified. Please verify your e-mail address by the membership e-mail sent to you upon signing up.";
            public const string ValidationError = "An error occurred while validating the user credentials. Please try again later.";
            public const string AuthenticationCodeSent = "An authentication code was sent to your e-mail address";
        }
        
        public struct CredentialsErrorMessages
        {
            public const string EmailAddressAlreadyExists = "E-mail address already exists!";
            public const string EmailAdddressDoesNotExist = "A user with the entered e-mail address does not exist.";
            public const string EmailAddressInvalid = "E-mail address is invalid!";
            public const string EmailAddressMissing = "E-mail address was not provided";
            public const string PasswordMissing = "Password is not provided";
            public const string PasswordRequirements = "Password should contain at least " +
                                                       "{0} characters, including at least " +
                                                       "{1} upper case character(s) and at least " +
                                                       "{2} digit(s)";
            public const string FullNameAlreadyExists = "This full name already exists!";
            public const string FullNameDoesNotExist = "Who Owes Who account with the entered full name does not exist.";
            public const string FullNameInValid = "Full name is not valid!";
        }

        public struct ChangePasswordErrorMessages
        {
            public const string SuccessfullyChanged = "Your password was successfully changed.";
            public const string ExistingPasswordInvalid = "The existing password is invalid.";
            public const string NewPasswordMatchExisting = "The new password should be different from the existing password";
            public const string UserNotFound = "User not found with the provided email address. Please try again.";
            public const string NewPasswordsDoNotMatch = "The new passwords does not match!";
            public const string ForExistingPassword = "For existing password:";
            public const string ForNewPassword = "For new password:";
            public const string ForNewRepeatedPassword = "For new password repeated:";
        }

        public struct PasswordRecoveryErrorMessages
        {
            public const string SuccessfullySent = "A password reset link sent to your e-mail address.";
            public const string UserNotFound = "User not found with the provided email address. Please try again.";
            public const string InvalidEmailAddress = "Invalid e-mail address provided.";
            public const string RequestInvalid = "emailAddress or forgotPasswordToken was not provided. Please, try again.";
            public const string TokenCreationError = "An error occurred while creating the forgot password token:";
            public const string TokenDeletionError = "An error occurred while deleting the forgot password token:";
        }

        public struct ResetPasswordErrorMessages
        {
            public const string ResetSucceeded = "Your password was successfully reset.";
            public const string PasswordsDoNotMatch = "The passwords does not match!";
            public const string UserAccountNotFound = "Could not find the user account with e-mail address:";
            public const string NewPasswordSameAsExisting = "The new password cannot be the same as the existing password.";
            public const string ForNewPassword = "For new password:";
            public const string ForNewPasswordRepeated = "For new password repeated:";
            public const string ResetPasswordTokenInvalid = "The password reset token is invalid or has expired.";
            public const string ResetPasswordLinkException = "An error occurred while verifying reset password link.";
        }

        public struct UserCreationErrorMessages
        {
            public const string UserLoadingUnsuccessful = "An error occurred while creating the user. Please, try again.";
            public const string SendAccountConfirmationException = "An error occurred while sending the account confirmation message:";
            public const string DispatchUserException = "An error occurred while dispatching the user by service bus:";
            public const string FullNameRequred = "Full name is required.";
            public const string SignupSucceeded = "Sign up successful! An e-mail has been sent to you for your account verification.";
            public const string UserCreationErrror = "An error occurred while creating the user:";
            public const string MobilePhoneNumberRequired = "Mobile Phone Number is required.";
            public const string EmailAddressRequired = "E-mail address is required";
            public const string PasswordRequired = "Password is requuired";
        }

        public struct UserUpdatingErrorMessages 
        {
            public const string UpdateSucceeded = "Your profile was successfully updated.";
            public const string AdministratorAlreadyExisting = "The event you have assigned to already has an administrator.";
            public const string NoAdministratorExisting = "The event running is now left with no administrator. This is indeed not recommended as event and payment edit, delete and settlement are not available as these can only be performed by an administrator.";
            public const string EmailVerificationSucceeded = "Email address successfully verified.";

        }

        public struct EventErrorMessages
        {
            public const string EventIdMissing = "Event id was empty";
            public const string UserIdMissing = "User id was empty";
            public const string NameMissing = "Event name was mot specified";
            public const string LocationMissing = "Location was not specified";
            public const string CurrencyMissing = "Currency was not selected";
            public const string StartDateMissing = "Stard dat was not selected";
            public const string ActiveEventsInavailable = "No active events are available.";
            public const string EventCreationSucceeded = "Event created successfully.";
            public const string EventModificationSucceeded = "Event updated successfully.";
            public const string EventDeletionSucceeded = "Event deleted successfully.";
            public const string UserAssignmentInvalid = "You cannot assign to this event as an administrator, because an event administrator already exists.";
            public const string UserAssignmentSucceeded = "Successfully assigned your user to the event.";
            public const string UserUnassignmentSucceeded = "Successfully unassigned user from the event.";
            public const string EventSettlmentSucceeded = "The event was successfully closed.";
            public const string EventUnsettlmentSucceeded = "The event was successfully reopened.";
        }

        public struct PaymentErrorMessages 
        {
            public const string PaymentInvalid = "Payment invalid as the only debtor is yourself, which does not make sense.";
            public const string PaymentAdditionSucceeded = "Payment added successfully.";
            public const string PaymentModificationSucceeded = "Payment updated successfully.";
            public const string PaymentsInavailable = "No payments available. Maybe the event has been settled (closed).";
            public const string PaymentsYetInavailable = "No payments has been made just yet.";
            public const string PaymentActiveInavailable = "No payments available. You are not assigned to an active event. That my be because that the event has been settled (closed).";
            public const string PamentRemovalSucceeded = "Payment deleted successfully";
        }

        public struct RequestArgumentErrorMessages
        {
            public const string TextArgumentError = "Text was not provided for protection/unprotection";
            public const string EmailArgumentError = "E-mail address argument was not provided.";
            public const string UserIdArgumentError = "User id was not provided";
            public const string ForgotPasswordTokenArgumentError = "Forgot password token was not provided";
            public const string CurrencyIsoError = "Currency iso argument was not provided";
            public const string PaymentCurrencyIsoError = "Payment currency iso was not provided";
            public const string EventCurrencyIsoError = "Event currency iso was not provided";
            public const string UserArgumentError = "User model was not provided";
            public const string UserIdsArgumentError = "Event user ids was not provided";
            public const string ForgotPasswordTokenError = "Forgot password token was not provided";
            public const string HostArgumentError = "Host was not provided";
            public const string TypeArgumentError = "Type was not provided";
            public const string CodeArgumentError = "Code wat not provided";
            public const string EventIdArgumentError = "Event id was not provided";
            public const string PaymentIdArgumentError = "Payment id was not provided";
            public const string TotalAmountArgumentError = "Total amount was not provided";
            public const string CreditorIdArgumentError = "Creditor id was not provided";
            public const string CurrencyArgumentEroor = "Currency was not provided";
            public const string OriginalCurrencyArgumentError = "Original currency was not provided";
            public const string OriginalAmountArgumentError = "Original amount was not provided";
            public const string CreatedbyArgumentError = "Events argument Created by was not provided";

        }

        public struct GlobalErrorMessages
        {
            public const string UnexpectedError = "An unexpected error occurred, please try again.";
            public const string HostRequired = "The Host argument is required";
            
        }
    }
}
