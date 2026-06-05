using System.Security.Cryptography;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IRefreshTokenService
    {
        Task<AuthorizationResponseModel> RefreshTokenAsync(RefreshRequestModel request);
        Task DeleteRefreshTokenAsync(string refreshToken);
    }

    public class RefreshTokenService(IAuthorizationService authorizationService, IAuthorizationCacheRepository authorizationCacheRepository) : IRefreshTokenService
    {
        public async Task<AuthorizationResponseModel> RefreshTokenAsync(RefreshRequestModel request)
        {
            var refreshToken = request.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new AuthorizationResponseModel
                {
                    Success = false,
                    Message = "Refresh token is required"
                };
            }

            var existingRefreshToken = await authorizationCacheRepository.GetRefreshTokenAsync(refreshToken);

            if (existingRefreshToken is null)
            {
                return new AuthorizationResponseModel
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            if (existingRefreshToken.ExpiresUtc < DateTime.UtcNow)
            {
                return new AuthorizationResponseModel
                {
                    Success = false,
                    Message = "Refresh token expired"
                };
            }

            var user = await authorizationCacheRepository.GetUserByIdAsync(existingRefreshToken.UserId.ToString());

            if (user is null)
            {
                return new AuthorizationResponseModel
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            //await authorizationCacheRepository.DeleteRefreshTokenAsync(refreshToken);

            var authorizationResponse = await authorizationService.AuthorizeAsync(
                        new AuthorizationRequestModel
                        {
                            EmailAddress = user.EmailAddress
                        });

            if (authorizationResponse is null || !authorizationResponse.Success)
            {
                return new AuthorizationResponseModel
                {
                    Success = false,
                    Message = "Failed to generate JWT"
                };
            }

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshModel = new RefreshTokenModel
            {
                UserId = user.Id,
                Token = newRefreshToken,
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(90)
            };
            await authorizationCacheRepository.SaveRefreshTokenAsync(refreshModel);
            authorizationResponse.RefreshValue = newRefreshToken;
            authorizationResponse.Success = true;
            return authorizationResponse;
        }

        public async Task DeleteRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }
            await authorizationCacheRepository.DeleteRefreshTokenAsync(refreshToken);
        }
    }

}
