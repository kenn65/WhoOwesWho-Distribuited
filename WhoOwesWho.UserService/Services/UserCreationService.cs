using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserCreationService
    {
        Task<UserModel?> CreateUserAsync(SignUpRequestModel request);
    }

    public class UserCreationService(
        IConfiguration configuration,
        IUserSecurityService userSecurityService,
        IUserValidationService userValidationService,
        IUserCommandService userCommandService
        ) : ServiceBase(configuration), IUserCreationService
    {
        public async Task<UserModel?> CreateUserAsync(SignUpRequestModel request)
        {
            var response = new UserModel();
            try
            {
                request.Entity!.EmailAddress = await userSecurityService.UnprotectAsync(request.Entity.EmailAddress!);

                if (string.IsNullOrWhiteSpace(request.Entity?.FullName))
                {
                    response.Message = "Full name is required.";
                    return response;
                }

                var emailCheck = await userValidationService.ValidateEmailAsync(request.Entity.EmailAddress!, false);
                if (!emailCheck.isValid)
                {
                    response.Message = emailCheck.errorMessage;
                    return response;
                }

                var passwordCheck = await userValidationService.ValidatePasswordAsync(request.Entity.Password!);
                if (!passwordCheck.isValid)
                {
                    response.Message = passwordCheck.errorMessage;
                    return response;
                }

                var check = await userCommandService.CreateUserAsync(request.Entity, request.Host!) is not null;
                response.Success = check;
                response.Message = !check
                    ? "An unexpected error occurred, please try again."
                    : "Sign up successful! An e-mail has been sent to you for your account verification.";
                return response;
            }
            catch
            {
                response.Message = "An unexpected error occurred, please try again.";
                return response;
            }
        }
    }
}
