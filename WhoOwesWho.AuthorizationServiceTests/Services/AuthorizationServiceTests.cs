using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Models;
using Xunit;

namespace WhoOwesWho.AuthorizationServiceTests.Services
{
    public class AuthorizationServiceTests
    {
        [Theory, AutoData]
        public async Task AuthorizeAsync_ReturnsProtectedAuthorizationResponse_WhenSuccessful(
            AuthorizationRequestModel request,
            UserMessageResponseModel user,
            AuthorizationResponseModel response,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            user.EmailAddress = request.EmailAddress;
            user.FullName = "John Doe";
            user.Admin = true;

            configuration
                .Setup(x => x["EncryptionMicroService:BaseAddress"])
                .Returns("https://localhost:7252/api/encryption");

            configuration
                .Setup(x => x["Authorization:JwtSecret"])
                .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.ProtectCookiesAsync(
                    user,
                    It.IsAny<string>(),
                    true))
                .ReturnsAsync(response);

            var sut = CreateAuthorizationService(
                cacheRepositoryMock: cacheRepositoryMock,
                securityServiceMock: securityServiceMock,
                configurationMock: configuration);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            result.Should().Be(response);

            securityServiceMock.Verify(
                x => x.ProtectCookiesAsync(
                    user,
                    It.Is<string>(token => !string.IsNullOrWhiteSpace(token)),
                    true),
                Times.Once);
        }

        [Theory, AutoData]
        public async Task AuthorizeAsync_CreatesJwtToken_WithExpectedClaims(
            AuthorizationRequestModel request,
            UserMessageResponseModel user,
            AuthorizationResponseModel response,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            user.FullName = "John Doe";
            user.EmailAddress = request.EmailAddress;
            user.Admin = true;

            string? capturedToken = null;

            configuration
                .Setup(x => x["Authorization:JwtSecret"])
                .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.ProtectCookiesAsync(
                    user,
                    It.IsAny<string>(),
                    true))
                .Callback<UserMessageResponseModel, string, bool>((_, token, _) =>
                {
                    capturedToken = token;
                })
                .ReturnsAsync(response);

            var sut = CreateAuthorizationService(
                cacheRepositoryMock: cacheRepositoryMock,
                securityServiceMock: securityServiceMock,
                configurationMock: configuration);

            // Act
            await sut.AuthorizeAsync(request);

            // Assert
            capturedToken.Should().NotBeNullOrWhiteSpace();
        }

        [Theory, AutoData]
        public async Task AuthorizeAsync_Throws_WhenUserLookupFails(
            AuthorizationRequestModel request)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ThrowsAsync(new Exception("Failure"));

            var sut = CreateAuthorizationService(
                cacheRepositoryMock: cacheRepositoryMock);

            // Act
            Func<Task> act = async () =>
                await sut.AuthorizeAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Failure");
        }

        [Theory, AutoData]
        public async Task AuthorizeAsync_Throws_WhenCookieProtectionFails(
            AuthorizationRequestModel request,
            UserMessageResponseModel user,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            configuration
                .Setup(x => x["EncryptionMicroService:BaseAddress"])
                .Returns("https://localhost:7252/api/encryption");

            configuration
                .Setup(x => x["Authorization:JwtSecret"])
                .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                    .ReturnsAsync(user);

            securityServiceMock
                .Setup(x => x.ProtectCookiesAsync(
                    user,
                    It.IsAny<string>(),
                        true))
                    .ThrowsAsync(new Exception("Protection failed"));

            var sut = CreateAuthorizationService(
                cacheRepositoryMock: cacheRepositoryMock,
                securityServiceMock: securityServiceMock,
                configurationMock: configuration);

            // Act
            Func<Task> act = async () =>
                await sut.AuthorizeAsync(request);

            // Assert
            await act.Should()
                    .ThrowAsync<Exception>()
                    .WithMessage("Protection failed");
        }

        private static AuthorizationService.Services.AuthorizationService CreateAuthorizationService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IAuthorizationCacheRepository>? cacheRepositoryMock = null,
            Mock<IAuthorizationSecurityService>? securityServiceMock = null)
        {
            configurationMock ??= new();
            cacheRepositoryMock ??= new();
            securityServiceMock ??= new();

            return new AuthorizationService.Services.AuthorizationService(
                configurationMock.Object,
                cacheRepositoryMock.Object,
                securityServiceMock.Object);
        }
    }
}