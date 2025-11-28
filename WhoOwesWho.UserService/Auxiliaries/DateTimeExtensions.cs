namespace WhoOwesWho.UserService.Auxiliaries
{
    public static class DateTimeExtensions
    {
        public static string ToIsoDateTimeFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddThh:mm:ss");
        }

        public static string ToDisplayDateTimeFormat(this DateTime value)
        {
            return value.ToString("dd-MM-yyyy hh:mm:ss");
        }

        public static string ToDisplayDateFormat(this DateTime value)
        {
            return value.ToString("dd-MM-yyyy");
        }

        public static string ToIsoDateFormat(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd");
        }
    }
}
