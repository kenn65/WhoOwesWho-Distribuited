using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Services.Gateways;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{
    public class UserSecurityServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ProtectAsync_CallsGateway_WhenValueIsValidGuid(
            string value,
            [Frozen] Mock<IEncryptionGatewayService> gateway,
            UserSecurityService sut)
        {
            // Arrange
            value = Guid.NewGuid().ToString();

            gateway
                .Setup(x => x.ProtectAsync(value, true))
                .ReturnsAsync("protected");

            // Act
            var result = await sut.ProtectAsync(value);

            // Assert
            result.Should().Be("protected");

            gateway.Verify(x =>
                x.ProtectAsync(value, true),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task ProtectAsync_CallsGateway_WhenValueIsValidEmailAddress(
            string value,
            [Frozen] Mock<IEncryptionGatewayService> gateway,
            UserSecurityService sut)
        {
            // Arrange
            value = "email@test.com";

            gateway
                .Setup(x => x.ProtectAsync(value, true))
                .ReturnsAsync("protected");

            // Act
            var result = await sut.ProtectAsync(value);

            // Assert
            result.Should().Be("protected");

            gateway.Verify(x =>
                x.ProtectAsync(value, true),
                Times.Once);
        }

        [Theory, AutoMoqData]
        // Not protecting guid nor e-mail
        public async Task ProtectAsync_CallsGateway_WhenValueMustBeForced(
           string value,
           [Frozen] Mock<IEncryptionGatewayService> gateway,
           UserSecurityService sut)
        {
            // Arrange
            value = "valid-value";

            gateway
                .Setup(x => x.ProtectAsync(value, true))
                .ReturnsAsync("protected");

            // Act
            var result = await sut.ProtectAsync(value, true);

            // Assert
            result.Should().Be("protected");

            gateway.Verify(x =>
                x.ProtectAsync(value, true),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task ProtectAsync_ReturnsValue_WhenValueIsNotValid(
            [Frozen] Mock<IEncryptionGatewayService> gateway,
            UserSecurityService sut)
        {
            // Arrange
            var value = "";

            // Act
            var result = await sut.ProtectAsync(value);

            // Assert
            result.Should().Be(value);

            gateway.Verify(x =>
                x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task ProtectAsync_ReturnsValue_WhenValueThrowsException(
            UserSecurityService sut)
        {
            // Arrange
            string? value = null;

            // Act
            Func<Task> act = () => sut.ProtectAsync(value!);

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();

            exception.Which.Message.Should().Be("Invalid value entered");
        }

        [Theory, AutoMoqData]
        public async Task UnprotectAsync_CallsGateway_WhenValueIsValid(
           string value,
           [Frozen] Mock<IEncryptionGatewayService> gateway,
           UserSecurityService sut)
        {
            // Arrange
            value = "valid-value";
            gateway
                .Setup(x => x.UnprotectAsync(value, true))
                .ReturnsAsync("unprotected");

            // Act
            var result = await sut.UnprotectAsync(value);

            // Assert
            result.Should().Be("unprotected");
            //result1.Should().Be("protected");

            gateway.Verify(x =>
                x.UnprotectAsync(value, true),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task UnprotectAsync_CallsGateway_WhenValueMustBeForced(
           string value,
           [Frozen] Mock<IEncryptionGatewayService> gateway,
           UserSecurityService sut)
        {
            // Arrange
            value = "valid-value";

            gateway
                .Setup(x => x.UnprotectAsync(value, true))
                .ReturnsAsync("unprotected");

            // Act
            var result = await sut.UnprotectAsync(value, true);

            // Assert
            result.Should().Be("unprotected");

            gateway.Verify(x =>
                x.UnprotectAsync(value, true),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task UnprotectAsync_ReturnsValue_WhenValueThrowsException(
            UserSecurityService sut)
        {
            // Arrange
            string? value = null;

            // Act
            Func<Task> act = () => sut.UnprotectAsync(value!);

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();

            exception.Which.Message.Should().Be("Invalid value entered");
        }
    }
}
