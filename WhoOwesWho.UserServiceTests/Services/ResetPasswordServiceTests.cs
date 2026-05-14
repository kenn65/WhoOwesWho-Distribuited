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
    public class ResetPasswordServiceTests
    {
        [Theory, AutoData]
        public async Task ResetPasswordAsync_Throws_WhenUserDoesNotExist(
            ResetPasswordRequestModel request)
        {
            // Arrange
            request.EmailAddress = "john@test.com";
            request.NewPassword = "Password1";
            request.NewPasswordRepeat = "Password1";

            var lookupServiceMock = new Mock<IUserLookupService>();

            lookupServiceMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateResetPasswordService(
                lookupServiceMock: lookupServiceMock);

            // Act
            Func<Task> act = async () =>
                await sut.ResetPasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage($"{Constants.ResetPasswordErrorMessages.UserAccountNotFound} {request.EmailAddress}");
        }

        [Theory, AutoData]
        public async Task ResetPasswordAsync_ReturnsError_WhenPasswordMatchesExistingPassword(
            ResetPasswordRequestModel request,
            UserModel user)
        {
            // Arrange
            request.EmailAddress = "john@test.com";
            request.NewPassword = "Password1";
            request.NewPasswordRepeat = "Password1";

            user.Password = "ProtectedPassword";

            var lookupServiceMock = new Mock<IUserLookupService>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            lookupServiceMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.UnprotectAsync(user.Password, false))
                .ReturnsAsync(request.NewPassword);

            var sut = CreateResetPasswordService(
                lookupServiceMock: lookupServiceMock,
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.ResetPasswordAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.ResetPasswordErrorMessages.NewPasswordSameAsExisting);
        }

        [Theory, AutoData]
        public async Task ResetPasswordAsync_ReturnsSuccess_WhenPasswordResetSucceeds(
            ResetPasswordRequestModel request,
            UserModel user,
            UserModel updatedUser)
        {
            // Arrange
            request.EmailAddress = "john@test.com";
            request.NewPassword = "NewPassword";
            request.NewPasswordRepeat = "NewPassword";

            user.Password = "ProtectedPassword";

            var lookupServiceMock = new Mock<IUserLookupService>();
            var commandServiceMock = new Mock<IUserCommandService>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            lookupServiceMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.UnprotectAsync(user.Password, false))
                .ReturnsAsync("OldPassword");

            commandServiceMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync(updatedUser);

            var sut = CreateResetPasswordService(
                lookupServiceMock: lookupServiceMock,
                commandServiceMock: commandServiceMock,
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.ResetPasswordAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeTrue();

            result.Message.Should()
                .Be(Constants.ResetPasswordErrorMessages.ResetSucceeded);
        }

        [Theory, AutoData]
        public async Task ResetPasswordAsync_Throws_WhenUpdateFails(
            ResetPasswordRequestModel request,
            UserModel user)
        {
            // Arrange
            request.EmailAddress = "john@test.com";
            request.NewPassword = "NewPassword";
            request.NewPasswordRepeat = "NewPassword";

            user.Password = "ProtectedPassword";

            var lookupServiceMock = new Mock<IUserLookupService>();
            var commandServiceMock = new Mock<IUserCommandService>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            lookupServiceMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.UnprotectAsync(user.Password, false))
                .ReturnsAsync("OldPassword");

            commandServiceMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateResetPasswordService(
                lookupServiceMock: lookupServiceMock,
                commandServiceMock: commandServiceMock,
                securityServiceMock: securityServiceMock);

            // Act
            Func<Task> act = async () =>
                await sut.ResetPasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage($"{Constants.ResetPasswordErrorMessages.UserAccountNotFound} {request.EmailAddress}");
        }

        [Theory, AutoData]
        public async Task VerifyResetPassword_ReturnsSuccess_WhenTokenIsValid(
            string emailAddress,
            string token,
            UserModel user,
            ForgotPasswordTokenModel tokenResponse)
        {
            // Arrange
            user.Id = Guid.NewGuid();

            tokenResponse.ForgotPasswordToken = token;
            tokenResponse.ExpirationTime = DateTime.Now.AddHours(1).Ticks;

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync(token);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(emailAddress, true))
                .ReturnsAsync(user);

            queryRepositoryMock
                .Setup(x => x.GetForgotPasswordTokenAsync(user.Id))
                .ReturnsAsync(tokenResponse);

            var sut = CreateResetPasswordService(
                queryRepositoryMock: queryRepositoryMock,
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.VerifyResetPassword(emailAddress, token);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task VerifyResetPassword_Throws_WhenTokenDoesNotMatch(
            string emailAddress,
            string token,
            UserModel user,
            ForgotPasswordTokenModel tokenResponse)
        {
            // Arrange
            user.Id = Guid.NewGuid();

            tokenResponse.ForgotPasswordToken = "DifferentToken";
            tokenResponse.ExpirationTime = DateTime.Now.AddHours(1).Ticks;

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            securityServiceMock
                .SetupSequence(x => x.UnprotectAsync(tokenResponse.ForgotPasswordToken))
                .ReturnsAsync("AnotherToken");

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(emailAddress, true))
                .ReturnsAsync(user);

            queryRepositoryMock
                .Setup(x => x.GetForgotPasswordTokenAsync(user.Id))
                .ReturnsAsync(tokenResponse);

            var sut = CreateResetPasswordService(
                queryRepositoryMock: queryRepositoryMock,
                securityServiceMock: securityServiceMock);

            // Act
            Func<Task> act = async () =>
                await sut.VerifyResetPassword(emailAddress, token);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.ResetPasswordErrorMessages.ResetPasswordTokenInvalid);
        }

        [Theory, AutoData]
        public async Task VerifyResetPassword_Throws_WhenTokenExpired(
            string emailAddress,
            string token,
            UserModel user,
            ForgotPasswordTokenModel tokenResponse)
        {
            // Arrange
            user.Id = Guid.NewGuid();

            tokenResponse.ForgotPasswordToken = token;
            tokenResponse.ExpirationTime = DateTime.Now.AddHours(-1).Ticks;

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var securityServiceMock = new Mock<IUserSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync(token);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(emailAddress, true))
                .ReturnsAsync(user);

            queryRepositoryMock
                .Setup(x => x.GetForgotPasswordTokenAsync(user.Id))
                .ReturnsAsync(tokenResponse);

            var sut = CreateResetPasswordService(
                queryRepositoryMock: queryRepositoryMock,
                securityServiceMock: securityServiceMock);

            // Act
            Func<Task> act = async () =>
                await sut.VerifyResetPassword(emailAddress, token);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.ResetPasswordErrorMessages.ResetPasswordTokenInvalid);
        }

        private static ResetPasswordService CreateResetPasswordService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IUserLookupService>? lookupServiceMock = null,
            Mock<IUserQueryRepository>? queryRepositoryMock = null,
            Mock<IUserCommandService>? commandServiceMock = null,
            Mock<IUserSecurityService>? securityServiceMock = null)
        {
            configurationMock ??= new();
            lookupServiceMock ??= new();
            queryRepositoryMock ??= new();
            commandServiceMock ??= new();
            securityServiceMock ??= new();

            return new ResetPasswordService(
                configurationMock.Object,
                lookupServiceMock.Object,
                queryRepositoryMock.Object,
                commandServiceMock.Object,
                securityServiceMock.Object);
        }
    }
}