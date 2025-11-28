using Microsoft.AspNetCore.Http;

namespace WhoOwesWho.Models.Models.Extensions
{
    public static class TokenExtension
    {
        public static string ToTokenValue(this HttpContext? context)
        {
            return context?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        }
    }
}
