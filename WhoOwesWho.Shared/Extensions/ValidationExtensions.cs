using Microsoft.Azure.Amqp;
using System.Text.RegularExpressions;

namespace WhoOwesWho.Shared.Extensions
{
    public static class ValidationExtensions
    {
        public static bool IsValid(this string emailAddress)
        {
            const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(emailAddress);
        }

        public static bool IsValid(this string password, string lengthRequired, string upperCaseRequired, string digitsRequuired)
        {
            return password!.Length >= int.Parse(lengthRequired)
            && password.Count(char.IsUpper) >= int.Parse(upperCaseRequired)
            && password.Count(char.IsDigit) >= int.Parse(digitsRequuired);
        }

        public static bool IsGuid(this string id)
        {
            return Guid.TryParse(id, out var guid);
        }
    }
}
