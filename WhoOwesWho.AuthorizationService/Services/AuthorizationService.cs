using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationService
    {
        Task<AuthorizationResponseModel?> Authorize(AuthorizationRequestModel request);
    }
    public class AuthorizationService(
        IConfiguration configuration,
        IEncryptionGatewayService encryptionGatewayService,
        IUserGatewayService userGatewayService
        ) : ServiceBase(configuration), IAuthorizationService
    {
        public async Task<AuthorizationResponseModel?> Authorize(AuthorizationRequestModel request)
        {
            var unprotectedEmailAddress = await encryptionGatewayService.UnprotectAsync(request.EmailAddress!, true);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, unprotectedEmailAddress!)
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

            var user = await userGatewayService.GetUserAsync(request.EmailAddress!, false);
            var response = await encryptionGatewayService.ProtectCookiesAsync(user, token, true);
            return await Task.FromResult(response);
        }
    }
}
