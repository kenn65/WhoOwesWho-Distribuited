using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;
using WhoOwesWho.UserServiceTests;
using Xunit;

namespace WhoOwesWho.UserService.Services
{
    public class UserNotificationServiceTests
    {
        [Theory, AutoMoqData]
        public async Task SendAccountConfirmationMessage_DispatchesMessage(
            UserMessageRequestModel entity,
            string host,
            [Frozen] Mock<IMessagingPublisher> publisher,
            UserNotificationService sut)
        {
            await sut.SendAccountConfirmationMessage(entity, host);

            publisher.Verify(x =>
                x.DispatchAsync(It.Is<MessagingRequestModel>(r =>
                    r.Type == "SignUp" &&
                    r.Host == host &&
                    r.User == entity)),
                Times.Once);
        }


        [Theory, AutoMoqData]
        public async Task SendAccountConfirmationMessage_ThrowsWrappedException_WhenDispatchFails(
            UserMessageRequestModel entity,
            string host,
            [Frozen] Mock<IMessagingPublisher> publisher,
            UserNotificationService sut)
        {
            publisher
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .ThrowsAsync(new Exception("fail"));

            Func<Task> act = async () => await sut.SendAccountConfirmationMessage(entity, host);

            var exception = await act.Should().ThrowAsync<Exception>();
            exception.Which.Message.Should().Contain("An error occurred while sending the account confirmation message");
        }


        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryMessage_CreatesToken_AndDispatchesMessage(
            UserMessageRequestModel entity,
            string host,
            string token,

            [Frozen] Mock<IConfiguration> configurationMock,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserMutationRepository> mutationRepo,
            [Frozen] Mock<IMessagingPublisher> publisher,

            UserNotificationService sut)
        {
            configurationMock
                .Setup(x => x["MessagingMicroService:Security:ApiKey"])
                .Returns("test-api-key");

            configurationMock
                .Setup(x => x["Password:ForgotPassword:ExpirationTimeInMinutes"])
                .Returns("30");
            
            securityService
                .Setup(x => x.ProtectAsync(token, true))
                .ReturnsAsync("protectedToken");

            mutationRepo
                .Setup(x => x.CreateForgotPasswordTokenAsync(It.IsAny<ForgotPasswordTokenModel>()))
                .ReturnsAsync(true);

            Func<Task> act = () => sut.SendPasswordRecoveryMessage(entity, host, token);

            await act.Should().NotThrowAsync();

            mutationRepo.Verify(x =>
                x.DeleteForgotPasswordTokenAsync(entity.Id),
                Times.Once);

            publisher.Verify(x =>
                x.DispatchAsync(It.Is<MessagingRequestModel>(r =>
                    r.Type == "ResetPassword" &&
                    r.Host == host &&
                    r.User == entity &&
                    r.ForgotPasswordToken == "protectedToken")),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryMessage_Throws_WhenTokenCreationFails(
            UserMessageRequestModel entity,
            string host,
            string token,
            [Frozen] Mock<IConfiguration> configurationMock,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserMutationRepository> mutationRepo,
            UserNotificationService sut)
        {
            configurationMock
                .Setup(x => x["MessagingMicroService:Security:ApiKey"])
                .Returns("test-api-key");

            configurationMock
                .Setup(x => x["Password:ForgotPassword:ExpirationTimeInMinutes"])
                .Returns("30");

            securityService
                .Setup(x => x.ProtectAsync(token, true))
                .ReturnsAsync("protectedToken");

            mutationRepo
                .Setup(x => x.CreateForgotPasswordTokenAsync(It.IsAny<ForgotPasswordTokenModel>()))
                .ReturnsAsync(false);

            Func<Task> act = async () =>
                await sut.SendPasswordRecoveryMessage(entity, host, token);

            var exception = await act.Should().ThrowAsync<Exception>();

            exception.Which.Message.Should().Contain("An error occurred while sending forgot password message");
            exception.Which.Message.Should().Contain("Failed to create forgot password token");
        }


        [Theory, AutoMoqData]
        public async Task SendPasswordRecoveryMessage_ThrowsWrappedException_WhenDispatchFails(
            UserMessageRequestModel entity,
            string host,
            string token,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserMutationRepository> mutationRepo,
            [Frozen] Mock<IMessagingPublisher> publisher,
            UserNotificationService sut)
        {
            securityService
                .Setup(x => x.ProtectAsync(token, true))
                .ReturnsAsync("protectedToken");

            mutationRepo
                .Setup(x => x.CreateForgotPasswordTokenAsync(It.IsAny<ForgotPasswordTokenModel>()))
                .ReturnsAsync(true);

            publisher
                .Setup(x => x.DispatchAsync(It.IsAny<MessagingRequestModel>()))
                .ThrowsAsync(new Exception("fail"));

            Func<Task> act = async () =>
                await sut.SendPasswordRecoveryMessage(entity, host, token);

            var exception = await act.Should().ThrowAsync<Exception>();

            exception.Which.Message.Should().Contain("An error occurred while sending forgot password message");
        }
    }
}