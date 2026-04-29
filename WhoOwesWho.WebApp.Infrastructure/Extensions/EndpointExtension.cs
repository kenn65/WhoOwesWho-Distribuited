namespace WhoOwesWho.WebApp.Infrastructure.Extensions
{
    public static class EndpointExtension
    {
        public static async Task<string> ToEndpointAsync(this string baseAddress, string trailingPath)
        {
            if (string.IsNullOrWhiteSpace(trailingPath))
            {
                return baseAddress;
            }
            return $"{baseAddress}/{trailingPath}";
        }
    }
}
