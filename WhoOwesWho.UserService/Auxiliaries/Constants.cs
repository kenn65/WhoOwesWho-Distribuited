namespace WhoOwesWho.UserService.Auxiliaries
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

        public struct CredentialsErrorMessages
        {
            public const string EmailAddressAlreadyExists = "E-mail address already exists!";
            public const string EmailAdddressDoesNotExist = "Who Owes Who account with the entered e-mail address does not exist.";
            public const string EmailAddressInvalidValid = "E-mail address is not valid!";
            public const string PasswordRequirements = "Password should contain at least " +
                                                       "{0} characters, including at least " +
                                                       "{1} upper case character(s) and at least " +
                                                       "{2} digit(s)";
            public const string FullNameAlreadyExists = "This full name already exists!";
            public const string FullNameDoesNotExist = "Who Owes Who account with the entered full name does not exist.";
            public const string FullNameInValid = "Full name is not valid!";
        }
    }
}
