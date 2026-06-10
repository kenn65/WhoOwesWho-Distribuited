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
        public async Task AuthorizeAsync_ReturnsAuthorizationResponse_WhenSuccessful(
             AuthorizationRequestModel request,
             UserMessageResponseModel user,
             [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            user.EmailAddress = request.EmailAddress;
            user.FullName = "John Doe";
            user.Admin = true;

            configuration
                .Setup(x => x["Authorization:JwtSecret"])
                .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ReturnsAsync(user);

            var sut = CreateAuthorizationService(
                cacheRepositoryMock: cacheRepositoryMock,
                configurationMock: configuration);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeTrue();

            result.TokenValue.Should().NotBeNullOrWhiteSpace();

            result.RefreshValue.Should().NotBeNullOrWhiteSpace();

            cacheRepositoryMock.Verify(
                x => x.GetUserAsync(request.EmailAddress!),
                Times.Once);

            cacheRepositoryMock.Verify(
                x => x.SaveRefreshTokenAsync(It.IsAny<RefreshTokenModel>()),
                Times.Once);
        }

        [Theory, AutoData]
        public async Task AuthorizeAsync_CreatesJwtToken_WithExpectedClaims(
            AuthorizationRequestModel request,
            UserMessageResponseModel user,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            user.FullName = "John Doe";
            user.EmailAddress = request.EmailAddress;
            user.Admin = true;
            
            configuration
                .Setup(x => x["Authorization:JwtSecret"])
                .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();
            
            cacheRepositoryMock
                .Setup(x => x.GetUserAsync(request.EmailAddress!))
                .ReturnsAsync(user);

            var sut = CreateAuthorizationService(
                cacheRepositoryMock: cacheRepositoryMock,
                configurationMock: configuration);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            result?.TokenValue.Should().NotBeNullOrWhiteSpace();
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

       
        private static AuthorizationService.Services.AuthorizationService CreateAuthorizationService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IAuthorizationCacheRepository>? cacheRepositoryMock = null)
            
        {
            configurationMock ??= new();
            cacheRepositoryMock ??= new();
            

            return new AuthorizationService.Services.AuthorizationService(
                configurationMock.Object,
                cacheRepositoryMock.Object);
        }
    }
}