using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using Xunit;

namespace WhoOwesWho.AuthorizationService.Tests.Services
{
    public class AuthenticationNotificationServiceTests
    {
        private static AuthenticationNotificationService CreateSut(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IAuthorizationCacheRepository>? cacheRepositoryMock = null,
            Mock<IMessagingPublisher>? messagingPublisherMock = null)
        {
            configurationMock ??= new();
            cacheRepositoryMock ??= new();
            messagingPublisherMock ??= new();

            return new AuthenticationNotificationService(
                configurationMock.Object,
                cacheRepositoryMock.Object,
                messagingPublisherMock.Object);
        }

        [Theory, AutoData]
        public async Task SendAuthenticationMessageAsync_ReturnsSuccess_WhenMessageIsSent(
            AuthenticationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            request.EmailAddress = "john@test.com";
            request.Host = "localhost";

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            var messagingPublisherMock =
                new Mock<IMessagingPublisher>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress))
                .ReturnsAsync(user);

            messagingPublisherMock
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut(
                cacheRepositoryMock: cacheRepositoryMock,
                messagingPublisherMock: messagingPublisherMock);

            // Act
            var result = await sut.SendAuthenticationMessageAsync(request);

            // Assert
            result.Should().NotBeNull();

            result.Success.Should().BeTrue();

            result.Message.Should()
                .Be(Constants.AuthenticationErrorMessages.AuthenticationCodeSent);

            result.Code.Should().NotBeNullOrWhiteSpace();

            messagingPublisherMock.Verify(
                x => x.DispatchAsync(
                    It.Is<MessagingRequestModel>(m =>
                        m.Host == request.Host &&
                        m.Type == "Authentication" &&
                        m.User != null &&
                        !string.IsNullOrWhiteSpace(m.Code))),
                Times.Once);
        }

        [Theory, AutoData]
        public async Task SendAuthenticationMessageAsync_Throws_WhenUserLookupFails(
            AuthenticationRequestModel request)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ThrowsAsync(new Exception(Constants.GlobalErrorMessages.UnexpectedError));

            var sut = CreateSut(
                cacheRepositoryMock: cacheRepositoryMock);

            // Act
            Func<Task> act = async () =>
                await sut.SendAuthenticationMessageAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task SendAuthenticationMessageAsync_Throws_WhenDispatchFails(
            AuthenticationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            var messagingPublisherMock =
                new Mock<IMessagingPublisher>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ReturnsAsync(user);

            messagingPublisherMock
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .ThrowsAsync(new Exception(Constants.GlobalErrorMessages.UnexpectedError));

            var sut = CreateSut(
                cacheRepositoryMock: cacheRepositoryMock,
                messagingPublisherMock: messagingPublisherMock);

            // Act
            Func<Task> act = async () =>
                await sut.SendAuthenticationMessageAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task SendAuthenticationMessageAsync_GeneratesAuthenticationCode(
            AuthenticationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            MessagingRequestModel? dispatchedRequest = null;

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            var messagingPublisherMock =
                new Mock<IMessagingPublisher>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ReturnsAsync(user);

            messagingPublisherMock
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .Callback<MessagingRequestModel>(x => dispatchedRequest = x)
                .Returns(Task.CompletedTask);

            var sut = CreateSut(
                cacheRepositoryMock: cacheRepositoryMock,
                messagingPublisherMock: messagingPublisherMock);

            // Act
            await sut.SendAuthenticationMessageAsync(request);

            // Assert
            dispatchedRequest.Should().NotBeNull();

            dispatchedRequest!.Code.Should()
                .NotBeNullOrWhiteSpace();

            dispatchedRequest.Code.Should()
                .MatchRegex(@"^\d{5,6}$");
        }
    }
}