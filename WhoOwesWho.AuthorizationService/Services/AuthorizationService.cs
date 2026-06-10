using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationService
    {
        Task<AuthorizationResponseModel?> AuthorizeAsync(AuthorizationRequestModel request);
    }
    public class AuthorizationService(
        IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository
        ) : ServiceBase(configuration), IAuthorizationService
    {
        public async Task<AuthorizationResponseModel?> AuthorizeAsync(AuthorizationRequestModel request)
        {
            var user = await authorizationCacheRepository.GetUserAsync(request.EmailAddress!);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user!.Id.ToString()),
                new(ClaimTypes.Email, user.EmailAddress!),
                new(ClaimTypes.Name, user.FullName!),
                new(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.AuthorizationJwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: AppSettings.AuthorizationIssuer,
                audience: AppSettings.AuthorizationAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(2),
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            var refresh = await GenerateRefreshToken(user);
            return new AuthorizationResponseModel
            {
                TokenValue = token,
                RefreshValue = refresh,
                Success = true
            };
        }

        private async Task<string> GenerateRefreshToken(UserMessageResponseModel user)
        {
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshModel =
                new RefreshTokenModel
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    CreatedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddDays(1)
                };

            await authorizationCacheRepository.SaveRefreshTokenAsync(refreshModel);
            return newRefreshToken;
        }
    }
}
