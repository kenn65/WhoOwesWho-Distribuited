using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Encryption;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Messaging;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationService
    {
        Task<AuthorizationResponseModel?> Authorize(AuthorizationRequestModel request);
    }
    public class AuthorizationService(
        IConfiguration configuration,
        IUnprotectValueMessageSender unprotectValueMessageSender,
        IProtectCookiesMessageSender protectCookiesMessageSender,
        IUserMessageSender userMessageSender) : ServiceBase(configuration), IAuthorizationService
    {
        public async Task<AuthorizationResponseModel?> Authorize(AuthorizationRequestModel request)
        {
            var unprotectedEmailAddress = await unprotectValueMessageSender.SendAsync(new UnprotectValueRequestModel {
                ApiKey = AppSettings.EncryptionMicroServiceApiKey,
                Text = request.EmailAddress! 
            });
            
            //var unprotectedEmailAddress = await encryptionGatewayService.UnprotectAsync(request.EmailAddress!, true);
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

            var user = await userMessageSender.SendAsync(new UserRequestModel
            {
                ApiKey =  AppSettings.UserMicroServiceApiKey,
                IdOrEmailAddress = request.EmailAddress!,
                IncludePassword = true
            });
                      
            var userResponse = await protectCookiesMessageSender.SendAsync(new CookiesRequestModel
            {
                User = user
            });
                        
            var response = new AuthorizationResponseModel
            {
                TokenValue = token,
                UserIdValue = userResponse.UserIdValue,
                UserEmailAddressValue = userResponse.UserEmailAddressValue,
                AdminValue = userResponse.AdminValue,
                Success = true
            };
            return await Task.FromResult(response);
        }
    }
}
