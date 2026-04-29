namespace WhoOwesWho.WebApp.CoreBusiness.Extensions
{
    using System.Globalization;

    namespace WhoOwesWho.Shared.Extensions
    {
        public static class FormatAmountExtension
        {
            public static string FormatAmount(this decimal amount)
            {
                var valueString = amount.ToString(CultureInfo.CurrentCulture);
                var activeCulture = CultureInfo.CurrentCulture;
                var decimalSeparator = activeCulture.NumberFormat.NumberDecimalSeparator;
                var groupSeparator = activeCulture.NumberFormat.NumberGroupSeparator;

                if (decimalSeparator == "," && valueString!.Contains('.') && !valueString.Contains(','))
                {
                    return valueString.Replace('.', ',');
                }

                if (decimalSeparator == "." && valueString!.Contains(',') && !valueString.Contains('.'))
                {
                    return valueString.Replace(',', '.');
                }
                valueString = amount.ToString($"N{2}", activeCulture);
                return valueString;
            }

            public static string FormatAmount(this string amount)
            {
                var activeCulture = CultureInfo.CurrentCulture;
                var decimalSeparator = activeCulture.NumberFormat.NumberDecimalSeparator;
                var groupSeparator = activeCulture.NumberFormat.NumberGroupSeparator;

                if (decimalSeparator == "," && amount.Contains('.') && !amount.Contains(','))
                {
                    return amount.Replace('.', ',');
                }

                if (decimalSeparator == "." && amount.Contains(',') && !amount.Contains('.'))
                {
                    return amount.Replace(',', '.');
                }
                return amount;
            }
        }
    }
}
