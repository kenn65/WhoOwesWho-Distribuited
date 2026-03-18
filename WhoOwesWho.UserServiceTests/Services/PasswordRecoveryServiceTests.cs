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
    public class PasswordRecoveryServiceTests
    {
        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryEmailAsync_ShouldReturnError_WhenEmailIsInvalid(
            ForgotPasswordRequestModel request,
            [Frozen] Mock<IUserSecurityService> security,
            PasswordRecoveryService service)
        {
            request.Host = "https://host.com";

            security
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("invalid-email");

            var result = await service.SendPasswordRecoveryEmailAsync(request);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid e-mail address provided.");
        }

        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryEmailAsync_ShouldReturnError_WhenHostMissing(
            ForgotPasswordRequestModel request,
            [Frozen] Mock<IUserSecurityService> security,
            PasswordRecoveryService service)
        {
            request.Host = "";

            security
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("user@test.com");

            var result = await service.SendPasswordRecoveryEmailAsync(request);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Host is not provided.");
        }

        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryEmailAsync_ShouldReturnError_WhenValidationFails(
            ForgotPasswordRequestModel request,
            [Frozen] Mock<IUserSecurityService> security,
            [Frozen] Mock<IUserValidationService> validation,
            PasswordRecoveryService service)
        {
            request.Host = "https://host.com";

            security
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("user@test.com");

            validation
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
                .ReturnsAsync((false, "Invalid email"));

            var result = await service.SendPasswordRecoveryEmailAsync(request);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid email");
        }

        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryEmailAsync_ShouldReturnError_WhenUserNotFound(
            ForgotPasswordRequestModel request,
            [Frozen] Mock<IUserSecurityService> security,
            [Frozen] Mock<IUserValidationService> validation,
            [Frozen] Mock<IUserQueryRepository> repository,
            PasswordRecoveryService service)
        {
            request.Host = "https://host.com";

            security
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("user@test.com");

            validation
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
                .ReturnsAsync((true, ""));

            repository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(It.IsAny<string>(), false))
                .ReturnsAsync((UserModel?)null);

            var result = await service.SendPasswordRecoveryEmailAsync(request);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Could not find user, please try again.");
        }

        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryEmailAsync_ShouldReturnSuccess_WhenEmailSent(
            ForgotPasswordRequestModel request,
            UserModel user,
            [Frozen] Mock<IUserSecurityService> security,
            [Frozen] Mock<IUserValidationService> validation,
            [Frozen] Mock<IUserQueryRepository> repository,
            [Frozen] Mock<IUserNotificationService> notification,
            PasswordRecoveryService service)
        {
            request.Host = "https://host.com";

            security
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("user@test.com");

            validation
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
                .ReturnsAsync((true, ""));

            repository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(It.IsAny<string>(), false))
                .ReturnsAsync(user);

            security
                .Setup(x => x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("token");

            var result = await service.SendPasswordRecoveryEmailAsync(request);

            result.Success.Should().BeTrue();
            result.Message.Should().Be("A password reset link sent to your e-mail address.");

            notification.Verify(x =>
                x.SendPasswordRecoveryMessage(
                    It.IsAny<UserMessageRequestModel>(),
                    request.Host!,
                    "token"),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryEmailAsync_ShouldReturnError_WhenExceptionOccurs(
            ForgotPasswordRequestModel request,
            [Frozen] Mock<IUserSecurityService> security,
            PasswordRecoveryService service)
        {
            request.Host = "https://host.com";

            security
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());

            var result = await service.SendPasswordRecoveryEmailAsync(request);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred, please try again.");
        }
    }
}
