using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserService.Tests.Services
{
    public class PasswordRecoveryServiceTests
    {
        [Theory, AutoData]
        public async Task SendPasswordRecoveryEmailAsync_ReturnsSuccess_WhenEmailIsSent(
            ForgotPasswordRequestModel request,
            UserModel user,
            [Frozen] Mock<IConfiguration> configuration,
            string token)
        {
            // Arrange
            request.Host = "localhost";
            const string tokenSecret = "HEming_wayWas%here754";

            configuration
                .Setup(x => x["Password:ForgotPassword.TokenSecret"])
                .Returns(tokenSecret);
            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var notificationServiceMock = new Mock<IUserNotificationService>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, false))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.ProtectAsync(It.IsAny<string>()))
                .ReturnsAsync(token);

            notificationServiceMock
                .Setup(x => x.SendPasswordRecoveryMessage(
                    It.IsAny<UserMessageRequestModel>(),
                    request.Host,
                    token))
                .Returns(Task.CompletedTask);

            var sut = CreatePasswordRecoveryService(
                queryRepositoryMock: queryRepositoryMock,
                notificationServiceMock: notificationServiceMock,
                securityServiceMock: securityServiceMock,
                configurationMock: configuration);

            // Act
            var result = await sut.SendPasswordRecoveryEmailAsync(request);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeTrue();

            result.Message.Should()
                .Be(Constants.PasswordRecoveryErrorMessages.SuccessfullySent);

            notificationServiceMock.Verify(
                x => x.SendPasswordRecoveryMessage(
                    It.IsAny<UserMessageRequestModel>(),
                    request.Host,
                    token),
                Times.Once);
        }

        [Theory, AutoData]
        public async Task SendPasswordRecoveryEmailAsync_ReturnsError_WhenUserDoesNotExist(
            ForgotPasswordRequestModel request)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, false))
                .ReturnsAsync((UserModel?)null);

            var sut = CreatePasswordRecoveryService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.SendPasswordRecoveryEmailAsync(request);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task SendPasswordRecoveryEmailAsync_ReturnsError_WhenNotificationFails(
            ForgotPasswordRequestModel request,
            UserModel user,
            [Frozen] Mock<IConfiguration> configuration,
            string token)
        {
            // Arrange
            request.Host = "localhost";
            const string tokenSecret = "HEming_wayWas%here754";

            configuration
                .Setup(x => x["Password:ForgotPassword.TokenSecret"])
                .Returns(tokenSecret);

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var notificationServiceMock = new Mock<IUserNotificationService>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, false))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.ProtectAsync(It.IsAny<string>()))
                .ReturnsAsync(token);

            notificationServiceMock
                .Setup(x => x.SendPasswordRecoveryMessage(
                    It.IsAny<UserMessageRequestModel>(),
                    request.Host,
                    token))
                .ThrowsAsync(new Exception());

            var sut = CreatePasswordRecoveryService(
                queryRepositoryMock: queryRepositoryMock,
                notificationServiceMock: notificationServiceMock,
                securityServiceMock: securityServiceMock,
                configurationMock: configuration);

            // Act
            var result = await sut.SendPasswordRecoveryEmailAsync(request);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task SendPasswordRecoveryEmailAsync_ReturnsError_WhenTokenGenerationFails(
            ForgotPasswordRequestModel request,
            UserModel user,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            configuration
                .Setup(x => x["Password:ForgotPassword.TokenSecret"])
                .Returns("HEming_wayWas%here754");

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, false))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.ProtectAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var sut = CreatePasswordRecoveryService(
                queryRepositoryMock: queryRepositoryMock,
                securityServiceMock: securityServiceMock,
                configurationMock: configuration);

            // Act
            var result = await sut.SendPasswordRecoveryEmailAsync(request);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task SendPasswordRecoveryEmailAsync_ReturnsError_WhenRepositoryFails(
            ForgotPasswordRequestModel request)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, false))
                .ThrowsAsync(new Exception());

            var sut = CreatePasswordRecoveryService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.SendPasswordRecoveryEmailAsync(request);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.GlobalErrorMessages.UnexpectedError);
        }

        private static PasswordRecoveryService CreatePasswordRecoveryService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IUserQueryRepository>? queryRepositoryMock = null,
            Mock<IUserNotificationService>? notificationServiceMock = null,
            Mock<IUserSecurityService>? securityServiceMock = null)
        {
            configurationMock ??= new();
            queryRepositoryMock ??= new();
            notificationServiceMock ??= new();
            securityServiceMock ??= new();

            return new PasswordRecoveryService(
                configurationMock.Object,
                queryRepositoryMock.Object,
                notificationServiceMock.Object,
                securityServiceMock.Object);
        }
    }
}