using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{
    public class ResetPasswordServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ResetPasswordAsync_ReturnsError_WhenPasswordsDoNotMatch(
           ResetPasswordRequestModel request,
           [Frozen] Mock<IUserSecurityService> securityService,
           ResetPasswordService sut)
        {
            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("password1")
                .ReturnsAsync("password2");

            var result = await sut.ResetPasswordAsync(request);

            result!.Success.Should().BeFalse();
            result.Message.Should().Be("The passwords does not match!");
        }

        [Theory, AutoMoqData]
        public async Task ResetPasswordAsync_ReturnsError_WhenUserDoesNotExist(
           ResetPasswordRequestModel request,
           [Frozen] Mock<IUserSecurityService> securityService,
           [Frozen] Mock<IUserLookupService> userLookupService,
           ResetPasswordService sut)
        {
            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("password")
                .ReturnsAsync("password");

            userLookupService
                .Setup(x => x.GetSingleUserByEmailAddressAsync("email@test.com", true))
                .ReturnsAsync((UserModel?)null);

            var result = await sut.ResetPasswordAsync(request);

            result!.Success!.Should().BeFalse();
            result.Message.Should().Contain("Could not find the account");
        }

        [Theory, AutoMoqData]
        public async Task ResetPasswordAsync_ReturnsError_WhenNewPasswordEqualsOldPassword(
           ResetPasswordRequestModel request,
           UserModel user,
           [Frozen] Mock<IUserSecurityService> securityService,
           [Frozen] Mock<IUserLookupService> userLookupService,
           ResetPasswordService sut)
        {
            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("password")
                .ReturnsAsync("password")
                .ReturnsAsync("password");

            user.Password = "protectedPassword";

            userLookupService
                .Setup(x => x.GetSingleUserByEmailAddressAsync("email@test.com", true))
                .ReturnsAsync(user);

            var result = await sut.ResetPasswordAsync(request);

            result!.Success.Should().BeFalse();
            result.Message.Should().Be("The new password cannot be the same as the existing password.");
        }

        [Theory, AutoMoqData]
        public async Task ResetPasswordAsync_ReturnsError_WhenPasswordValidationFails(
           ResetPasswordRequestModel request,
           UserModel user,
           [Frozen] Mock<IUserSecurityService> securityService,
           [Frozen] Mock<IUserLookupService> userLookupService,
           [Frozen] Mock<IUserValidationService> validationService,
           ResetPasswordService sut)
        {
            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("newpassword")
                .ReturnsAsync("newpassword")
                .ReturnsAsync("oldpassword");

            userLookupService
                .Setup(x => x.GetSingleUserByEmailAddressAsync("email@test.com", true))
                .ReturnsAsync(user);

            validationService
                .Setup(x => x.ValidatePasswordAsync("newpassword"))
                .ReturnsAsync((false, "Password invalid"));

            var result = await sut.ResetPasswordAsync(request);

            result!.Success.Should().BeFalse();
            result.Message.Should().Contain("Password invalid");
        }

        [Theory, AutoMoqData]
        public async Task ResetPasswordAsync_ReturnsSuccess_WhenPasswordUpdated(
            ResetPasswordRequestModel request,
            UserModel user,
            UserModel updatedUser,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserLookupService> userLookupService,
            [Frozen] Mock<IUserValidationService> validationService,
            [Frozen] Mock<IUserCommandService> commandService,
            ResetPasswordService sut)
        {
            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("newpassword")
                .ReturnsAsync("newpassword")
                .ReturnsAsync("oldpassword");

            securityService
                .Setup(x => x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("protectedId");

            userLookupService
                .Setup(x => x.GetSingleUserByEmailAddressAsync("email@test.com", true))
                .ReturnsAsync(user);

            validationService
                .Setup(x => x.ValidatePasswordAsync("newpassword"))
                .ReturnsAsync((true, ""));

            commandService
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync(updatedUser);

            var result = await sut.ResetPasswordAsync(request);
            
            result!.Success.Should().BeTrue();
            result.Message.Should().Be("Your password was succesfully reset.");
        }

        [Theory, AutoMoqData]
        public async Task VerifyResetPassword_ReturnsSuccess_WhenTokenValid(
            UserModel user,
            ForgotPasswordTokenModel token,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            ResetPasswordService sut)
        {
            token.ForgotPasswordToken = "protectedToken";
            token.ExpirationTime = DateTime.Now.AddMinutes(10).Ticks;

            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("token")
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("token");

            queryRepository
                .Setup(x => x.GetSingleUserByEmailAddressAsync("email@test.com", true))
                .ReturnsAsync(user);

            queryRepository
                .Setup(x => x.GetForgotPasswordTokenAsync(user.Id))
                .ReturnsAsync(token);

            var result = await sut.VerifyResetPassword("protectedEmail", "protectedToken");

            result.Success.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task VerifyResetPassword_ReturnsError_WhenTokenInvalid(
           UserModel user,
           ForgotPasswordTokenModel token,
           [Frozen] Mock<IUserSecurityService> securityService,
           [Frozen] Mock<IUserQueryRepository> queryRepository,
           ResetPasswordService sut)
        {
            token.ForgotPasswordToken = "protectedToken";
            token.ExpirationTime = DateTime.Now.AddMinutes(-10).Ticks;

            securityService.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("token")
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("token");

            queryRepository
                .Setup(x => x.GetSingleUserByEmailAddressAsync("email@test.com", true))
                .ReturnsAsync(user);

            queryRepository
                .Setup(x => x.GetForgotPasswordTokenAsync(user.Id))
                .ReturnsAsync(token);

            var result = await sut.VerifyResetPassword("protectedEmail", "protectedToken");

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Your reset password link is invalid or expired.");
        }

        [Theory, AutoMoqData]
        public async Task VerifyResetPassword_ReturnsError_WhenExceptionOccurs(
        [Frozen] Mock<IUserSecurityService> securityService,
        ResetPasswordService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());

            var result = await sut.VerifyResetPassword("email", "token");

            result.Success!.Should().BeFalse();
            result.Message.Should().Be("An error occurred while verifying reset password link.");
        }
    }
}
