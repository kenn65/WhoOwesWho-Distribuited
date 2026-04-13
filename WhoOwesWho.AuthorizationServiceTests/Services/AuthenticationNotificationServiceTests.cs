using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using Xunit;


namespace WhoOwesWho.AuthorizationServiceTests.Services
{
    public class AuthenticationNotificationServiceTests
    {
        [Theory, AutoMoqData]
        public async Task SendAuthenticationMessage_ReturnsError_WhenEmailIsMissing(
              AuthenticationNotificationService sut,
              AuthenticationRequestModel request)
        {
            // Arrange
            request.EmailAddress = null;
            request.Password = "password";

            // Act
            var result = await sut.SendAuthenticationMessage(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("E-mail address or password was not provided");
        }

        [Theory, AutoMoqData]
        public async Task SendAuthenticationMessage_ReturnsError_WhenPasswordIsMissing(
            AuthenticationNotificationService sut,
            AuthenticationRequestModel request)
        {
            // Arrange
            request.EmailAddress = "test@test.com";
            request.Password = null;

            // Act
            var result = await sut.SendAuthenticationMessage(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("E-mail address or password was not provided");
        }

        [Theory, AutoMoqData]
        public async Task SendAuthenticationMessage_ReturnsError_WhenCredentialsInvalid(
            [Frozen] Mock<IAuthenticationValidationService> validationServiceMock,
            AuthenticationNotificationService sut,
            AuthenticationRequestModel request)
        {
            // Arrange
            request.EmailAddress = "test@test.com";
            request.Password = "password";

            validationServiceMock
                .Setup(x => x.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                .ReturnsAsync(false);

            // Act
            var result = await sut.SendAuthenticationMessage(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid e-mail and/or password entered.");
        }

        [Theory, AutoMoqData]
        public async Task SendAuthenticationMessage_ReturnsError_WhenUserNotFound(
            [Frozen] Mock<IAuthenticationValidationService> validationService,
            [Frozen] Mock<IAuthorizationSecurityService> securityService,
            [Frozen] Mock<IAuthorizationCacheRepository> repository,
            AuthenticationNotificationService sut,
            AuthenticationRequestModel request)
        {
            // Arrange
            request.EmailAddress = "encrypted@test.com";
            request.Password = "password";

            validationService
                .Setup(x => x.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                .ReturnsAsync(true);

            securityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress))
                .ReturnsAsync("decrypted@test.com");

            repository
                .Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync((UserMessageResponseModel?)null);

            // Act
            var result = await sut.SendAuthenticationMessage(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("was not found");
        }

        [Theory, AutoMoqData]
        public async Task SendAuthenticationMessage_SendsMessage_AndReturnsSuccess(
            [Frozen] Mock<IAuthenticationValidationService> validationService,
            [Frozen] Mock<IAuthorizationSecurityService> securityService,
            [Frozen] Mock<IAuthorizationCacheRepository> repository,
            [Frozen] Mock<IMessagingPublisher> messagingPublisher,
            AuthenticationNotificationService sut,
            AuthenticationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            request.EmailAddress = "encrypted@test.com";
            request.Password = "password";

            validationService
                .Setup(x => x.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                .ReturnsAsync(true);

            securityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress))
                .ReturnsAsync("decrypted@test.com");

            repository
                .Setup(x => x.GetUserAsync("decrypted@test.com"))
                .ReturnsAsync(user);

            MessagingRequestModel? capturedRequest = null;

            messagingPublisher
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .Callback<MessagingRequestModel>(req => capturedRequest = req)
                .Returns(Task.CompletedTask);

            // Act
            var result = await sut.SendAuthenticationMessage(request);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("An authentication code was sent to your e-mail address");
            result.Code.Should().NotBeNullOrEmpty();

            messagingPublisher.Verify(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()), Times.Once);

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Type.Should().Be("Authentication");
            capturedRequest.Code.Should().Be(result.Code);
        }

        [Theory, AutoMoqData]
        public async Task SendAuthenticationMessage_ThrowsWrappedException_WhenPublisherFails(
            [Frozen] Mock<IAuthenticationValidationService> validationService,
            [Frozen] Mock<IAuthorizationSecurityService> securityService,
            [Frozen] Mock<IAuthorizationCacheRepository> repository,
            [Frozen] Mock<IMessagingPublisher> messagingPublisher,
            AuthenticationNotificationService sut,
            AuthenticationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            request.EmailAddress = "encrypted@test.com";
            request.Password = "password";

            validationService
                .Setup(x => x.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                .ReturnsAsync(true);

            securityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress))
                .ReturnsAsync("decrypted@test.com");

            repository
                .Setup(x => x.GetUserAsync("decrypted@test.com"))
                .ReturnsAsync(user);

            messagingPublisher
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .ThrowsAsync(new Exception("Boom"));

            // Act
            Func<Task> act = async () => await sut.SendAuthenticationMessage(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("*An error occurred while sending the account confirmation message*");
        }
    }
}
