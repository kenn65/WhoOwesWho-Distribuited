using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Models;
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
        public async Task DoesEmailExist_ReturnsTrue_WhenPasswordValid(
            string emailAddress,
            string password)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
               .Setup(x => x.GetUserAsync(emailAddress))
               .ReturnsAsync(new UserMessageResponseModel
               {
                   Password = password
               });


            var sut = CreateAuthenticationValidationService(
                cacheRepositoryMock: cacheRepositoryMock); 

            // Act
            var result = await sut.IsPasswordValid(emailAddress, password);

            // Assert
            result.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task DoesEmailExist_ReturnsFalse_WhenPasswordInvalid(
            string emailAddress,
            string password)
        {
            // Arrange
            var cacheRepositoryMock =
                new Mock<IAuthorizationCacheRepository>();

            cacheRepositoryMock
               .Setup(x => x.GetUserAsync(emailAddress))
               .ReturnsAsync(new UserMessageResponseModel
               {
                   Password = "123456"
               });


            var sut = CreateAuthenticationValidationService(
                cacheRepositoryMock: cacheRepositoryMock);

            // Act
            var result = await sut.IsPasswordValid(emailAddress, password);

            // Assert
            result.Should().BeFalse();
        }

        private static AuthValidationService CreateAuthenticationValidationService(
           Mock<IAuthorizationCacheRepository>? cacheRepositoryMock = null,
           Mock<IAuthorizationSecurityService>? authSecurityServiceMock = null)
        {
            cacheRepositoryMock ??= new();
            authSecurityServiceMock ??= new();

            authSecurityServiceMock
               .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
               .ReturnsAsync((string value, bool _) => value);

            return new AuthValidationService(cacheRepositoryMock.Object,authSecurityServiceMock.Object);
        }

    }
}