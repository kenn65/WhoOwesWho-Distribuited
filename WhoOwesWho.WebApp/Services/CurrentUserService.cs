using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;

namespace WhoOwesWho.WebApp.Services
{
    public class CurrentUserService(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserService
    {
        private async Task<ClaimsPrincipal> GetUserAsync()
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User;
        }

        public async Task<bool> GetIsAuthorizedAsync()
        {
            var user = await GetUserAsync();
            return user.Identity?.IsAuthenticated ?? false;
        }

        public async Task<Guid> GetUserIdAsync()
        {
            var user = await GetUserAsync();
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        public async Task<string> GetEmailAddressAsync()
        {
            var user = await GetUserAsync();
            return user.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
        }

        public async Task<bool> GetIsAdminAsync()
        {
            var user = await GetUserAsync();
            return user.IsInRole("Admin");
        }
    }
}
