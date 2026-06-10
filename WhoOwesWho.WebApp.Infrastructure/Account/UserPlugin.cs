using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Account
{
    public class UserPlugin(IConfiguration configuration, ITokenService tokenService, NavigationManager nav) : ApiPluginClientBase(configuration, tokenService, nav), IUserPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<UserModel> SignUp(SignUpRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync();
            return await PutAsync<UserModel, SignUpRequestModel>(endpoint, request, apiKey!, true);
        }

        public async Task<UserModel> VerifyAccountAsync(VerificationRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("emailaddress/verify");
            return await PostAsync<UserModel, VerificationRequestModel>(endpoint, request, apiKey, true);
        }

        public async Task<UserModel> GetUserByIdAsync(Guid id, bool includePassword = true)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var complete = includePassword.ToString().ToLowerInvariant();
            var endpoint = await baseAddress.ToEndpointAsync($"{id}/{complete}");
            return await GetAsync<UserModel>(endpoint, apiKey, true, applyToken: true);
        }

        public async Task<ForgotPasswordResponseModel> ForgotPasswordAsync(ForgotPasswordRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("password/forgot");
            return await PostAsync<ForgotPasswordResponseModel, ForgotPasswordRequestModel>(endpoint, request, apiKey, true);
        }

        public async Task<ResetPasswordResponseModel> VerifyResetPasswordAsync(string emailAddress, string forgotPasswordToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"password/reset/verify/{emailAddress}/{forgotPasswordToken}");
            return await GetAsync<ResetPasswordResponseModel>(endpoint, apiKey, true);
        }
        public async Task<ResetPasswordResponseModel> ResetPasswordAsync(ResetPasswordRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("password/reset");
            return await PostAsync<ResetPasswordResponseModel, ResetPasswordRequestModel>(endpoint, request, apiKey, true);
        }
        public async Task<UserModel> UpdateUserAsync(UserUpdateRequestModel? requst)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync();
            return await PatchAsync<UserModel, UserUpdateRequestModel>(endpoint, requst!, apiKey, true, applyToken: true);
        }
        public async Task<ChangePasswordResponseModel> ChangePasswordAsync(ChangePasswordRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("password/change");
            return await PatchAsync<ChangePasswordResponseModel, ChangePasswordRequestModel>(endpoint, request, apiKey, true, applyToken: true);
        }

        public async Task<IsAdminResponseModel> GetIsAdminAsync(Guid id)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"isadmin/{id}");
            return await GetAsync<IsAdminResponseModel>(endpoint, apiKey, true, applyToken: true);
        }

        private async Task<string> GetBaseAddressAsync() => appSettings.UserMicroserviceBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.UserMicroserviceApiKey!;
    }
}
