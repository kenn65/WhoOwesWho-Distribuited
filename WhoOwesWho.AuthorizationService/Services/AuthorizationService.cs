using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationService
    {
        Task<AuthorizationResponseModel?> Authorize(AuthorizationRequestModel request);
    }
    public class AuthorizationService(
        IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository,
        IAuthorizationSecurityService authorizationSecurityService
        ) : ServiceBase(configuration), IAuthorizationService
    {
        public async Task<AuthorizationResponseModel?> Authorize(AuthorizationRequestModel request)
        {
            request.EmailAddress = await authorizationSecurityService.UnprotectAsync(request.EmailAddress!);
            var user = await authorizationCacheRepository.GetUserAsync(request.EmailAddress!);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user!.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.EmailAddress!),
                new(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
                new("admin", user.Admin.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.AuthorizationJwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: AppSettings.AuthorizationIssuer,
                audience: AppSettings.AuthorizationAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            var response = await authorizationSecurityService.ProtectCookiesAsync(user, token, true);
            return response;
        }
    }
}
