using System.Globalization;

namespace WhoOwesWho.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static CultureInfo CultureInfo => CultureInfo.CurrentCulture;

        public static string ToIsoDateTimeFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo);
        }

        public static string ToDisplayDateTimeFormat(this DateTime value)
        {
            return value.ToString(CultureInfo);
        }

        public static string ToDisplayDateFormat(this DateTime value)
        {
            var formattedDateParts = value.ToString(CultureInfo).Split(' ');
            return formattedDateParts[0];
        }
    }
}
