using System.Globalization;

namespace WhoOwesWho.Models.Models.Extensions
{
    public static class DateTimeExtensions
    {
        public static CultureInfo CultureInfo => new CultureInfo("da-DK");

        public static string ToIsoDateTimeFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo);
        }

        public static string ToDisplayDateTimeFormat(this DateTime value)
        {
            return value.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo);
        }

        public static string ToDisplayDateFormat(this DateTime value)
        {
            return value.ToString("dd-MM-yyyy", CultureInfo);
        }

        public static string ToIsoDateFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo);
        }
    }
}
