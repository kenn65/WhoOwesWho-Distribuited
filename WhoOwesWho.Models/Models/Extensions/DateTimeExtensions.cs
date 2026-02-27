using System.Globalization;

namespace WhoOwesWho.Models.Models.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToIsoDateTimeFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddThh:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string ToDisplayDateTimeFormat(this DateTime value)
        {
            return value.ToString("dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string ToDisplayDateFormat(this DateTime value)
        {
            return value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        public static string ToIsoDateFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
