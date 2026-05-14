using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using Xunit;

namespace WhoOwesWho.AuthorizationService.Tests.Services
{
    public class AuthenticationValidationServiceTests
    {
       
        [Theory, AutoData]
        public async Task DoesEmailExist_ReturnsTrue_WhenEmailExists(
            string emailAddress)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
                .Setup(x => x.GetUserExistAsync(emailAddress))
                .ReturnsAsync(true);

            var sut = CreateAuthenticationValidationService(
                cacheRepositoryMock: cacheRepositoryMock);

            // Act
            var result = await sut.DoesEmailExist(emailAddress);

            // Assert
            result.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task DoesEmailExist_ReturnsFalse_WhenEmailDoesNotExist(
            string emailAddress)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
                .Setup(x => x.GetUserExistAsync(emailAddress))
                .ReturnsAsync(false);

            var sut = CreateAuthenticationValidationService(
                cacheRepositoryMock: cacheRepositoryMock);

            // Act
            var result = await sut.DoesEmailExist(emailAddress);

            // Assert
            result.Should().BeFalse();
        }

        private static AuthValidationService CreateAuthenticationValidationService(
           Mock<IAuthorizationCacheRepository>? cacheRepositoryMock = null)
        {
            cacheRepositoryMock ??= new();

            return new AuthValidationService(
                cacheRepositoryMock.Object);
        }

    }
}