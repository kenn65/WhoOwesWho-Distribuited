using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using Xunit;

namespace WhoOwesWho.AuthorizationServiceTests.Services
{
    public class AuthorizationSecurityServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ProtectAsync_CallsGateway_WhenValueIsValid(
            [Frozen] Mock<IEncryptionGatewayService> encryptionGatewayService,
            AuthorizationSecurityService sut)
        {
            // Arrange
            var value = "valid@test.com";

            encryptionGatewayService
                .Setup(x => x.ProtectAsync(value, true))
                .ReturnsAsync("protected");

            // Act
            var result = await sut.ProtectAsync(value);

            // Assert
            result.Should().Be("protected");

            encryptionGatewayService.Verify(x => x.ProtectAsync(value, true), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task ProtectAsync_CallsGateway_WhenValueIsGuid(
            [Frozen] Mock<IEncryptionGatewayService> encryptionGatewayService,
            AuthorizationSecurityService sut)
        {
            // Arrange
            var value = Guid.NewGuid().ToString();

            encryptionGatewayService
                .Setup(x => x.ProtectAsync(value, true))
                .ReturnsAsync("protected");

            // Act
            var result = await sut.ProtectAsync(value);

            // Assert
            result.Should().Be("protected");
        }

        [Theory, AutoMoqData]
        public async Task ProtectAsync_ReturnsValue_WhenInvalidAndNotGuid(
            [Frozen] Mock<IEncryptionGatewayService> encryptionGatewayService,
            AuthorizationSecurityService sut)
        {
            // Arrange
            var value = "???";

            // Act
            var result = await sut.ProtectAsync(value);

            // Assert
            result.Should().Be(value);

            encryptionGatewayService.Verify(x => x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task UnprotectAsync_CallsGateway_WhenInvalidAndNotGuid(
            [Frozen] Mock<IEncryptionGatewayService> encryptionGatewayService,
            AuthorizationSecurityService sut)
        {
            // Arrange
            var value = "encrypted-value";

            encryptionGatewayService
                .Setup(x => x.UnprotectAsync(value, true))
                .ReturnsAsync("decrypted");

            // Act
            var result = await sut.UnprotectAsync(value);

            // Assert
            result.Should().Be("decrypted");

            encryptionGatewayService.Verify(x => x.UnprotectAsync(value, true), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task UnprotectAsync_ReturnsValue_WhenValid(
            [Frozen] Mock<IEncryptionGatewayService> encryptionGatewayService,
            AuthorizationSecurityService sut)
        {
            // Arrange
            var value = "valid@test.com";

            // Act
            var result = await sut.UnprotectAsync(value);

            // Assert
            result.Should().Be(value);

            encryptionGatewayService.Verify(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task UnprotectAsync_ReturnsValue_WhenGuid(
            AuthorizationSecurityService sut)
        {
            // Arrange
            var value = Guid.NewGuid().ToString();

            // Act
            var result = await sut.UnprotectAsync(value);

            // Assert
            result.Should().Be(value);
        }

        [Theory, AutoMoqData]
        public async Task ProtectCookiesAsync_DelegatesToGateway(
            [Frozen] Mock<IEncryptionGatewayService> encryptionGatewayService,
            AuthorizationSecurityService sut,
            UserMessageResponseModel user,
            AuthorizationResponseModel expected)
        {
            // Arrange
            var token = "token";

            encryptionGatewayService
                .Setup(x => x.ProtectCookiesAsync(user, token, true))
                .ReturnsAsync(expected);

            // Act
            var result = await sut.ProtectCookiesAsync(user, token, true);

            // Assert
            result.Should().Be(expected);

            encryptionGatewayService.Verify(x => x.ProtectCookiesAsync(user, token, true), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task ValidateApiKey_ReturnsFalse_WhenEmpty(
            AuthorizationSecurityService sut)
        {
            // Act
            var result = await sut.ValidateApiKey(string.Empty);

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateApiKey_ReturnsTrue_WhenMatch(
            [Frozen]Mock<IConfiguration> configuration,
            AuthorizationSecurityService sut)
        {
            const string apiKey = "xbefAAatHaiVx5LomnAgG1ll5NKGWS20Qj7sWCr1X51Agr3VKm587huRjctXoVIoPtEAe7OuIUd9fXUhwmlbOZpMSjcYJIrS1Lpv8FkQhJWPcZWmVIkeSkqIfVnSB4L1";

            // Arrange
            configuration
            .Setup(x => x["Security:ApiKey"])
            .Returns(apiKey);

            // Act
            var result = await sut.ValidateApiKey(apiKey);

            // Assert
            result.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task ValidateApiKey_ReturnsFalse_WhenNotMatch(
            AuthorizationSecurityService sut)
        {
            // Arrange
            var invalidKey = "wrong-key";

            // Act
            var result = await sut.ValidateApiKey(invalidKey);

            // Assert
            result.Should().BeFalse();
        }
    }
}
