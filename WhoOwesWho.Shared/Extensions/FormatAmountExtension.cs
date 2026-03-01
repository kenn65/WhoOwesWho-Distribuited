using System.Globalization;

namespace WhoOwesWho.Shared.Extensions
{
    public static class FormatAmountExtension
    {
        public static string FormatAmount(this decimal amount)
        {
            var cultureInfo = new CultureInfo("da-DK");
            return amount.ToString("#,###,##0.00", cultureInfo);
        }
    }
}
