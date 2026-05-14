using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserService.Tests.Services
{
    public class UserCreationServiceTests
    {
        private static UserCreationService CreateUserCreationService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IUserCommandService>? commandServiceMock = null)
        {
            configurationMock ??= new();
            commandServiceMock ??= new();

            return new UserCreationService(
                configurationMock.Object,
                commandServiceMock.Object);
        }

        [Theory, AutoData]
        public async Task CreateUserAsync_ReturnsSuccess_WhenUserIsCreated(
            SignUpRequestModel request,
            UserModel createdUser)
        {
            // Arrange
            request.Entity ??= new UserModel();
            request.Host = "localhost";

            var commandServiceMock = new Mock<IUserCommandService>();

            commandServiceMock
                .Setup(x => x.CreateUserAsync(request.Entity, request.Host))
                .ReturnsAsync(createdUser);

            var sut = CreateSut(
                commandServiceMock: commandServiceMock);

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeTrue();

            result.Message.Should()
                .Be(Constants.UserCreationErrorMessages.SignupSucceeded);
        }

        [Theory, AutoData]
        public async Task CreateUserAsync_ReturnsError_WhenUserCreationFails(
            SignUpRequestModel request)
        {
            // Arrange
            request.Entity ??= new UserModel();
            request.Host = "localhost";

            var commandServiceMock = new Mock<IUserCommandService>();

            commandServiceMock
                .Setup(x => x.CreateUserAsync(request.Entity, request.Host))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateSut(
                commandServiceMock: commandServiceMock);

            // Act
            Func<Task> act = async () =>
                await sut.CreateUserAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task CreateUserAsync_ReturnsError_WhenExceptionOccurs(
            SignUpRequestModel request)
        {
            // Arrange
            request.Entity ??= new UserModel();
            request.Host = "localhost";

            var commandServiceMock = new Mock<IUserCommandService>();

            commandServiceMock
                .Setup(x => x.CreateUserAsync(request.Entity, request.Host))
                .ThrowsAsync(new Exception(Constants.GlobalErrorMessages.UnexpectedError));

            var sut = CreateSut(
                commandServiceMock: commandServiceMock);

            // Act
            Func<Task> act = async () =>
                await sut.CreateUserAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.GlobalErrorMessages.UnexpectedError);
        }

        private static UserCreationService CreateSut(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IUserCommandService>? commandServiceMock = null)
        {
            configurationMock ??= new();
            commandServiceMock ??= new();

            return new UserCreationService(
                configurationMock.Object,
                commandServiceMock.Object);
        }
    }
}