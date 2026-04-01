using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{
    public class UserPublishingServiceTests
    {
        [Theory, AutoMoqData]
        public async Task SendUserAsync_SetsApiKey_AndDispatchesMessage(
             IFixture fixture,
             UserMessageRequestModel user,
             [Frozen] Mock<IUserPublisher> publisher)
        {

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AuthorizationMicroService:Security:ApiKey"] = "test-api-key"
                })
                .Build();

            // Inject BEFORE creating SUT
            fixture.Inject<IConfiguration>(configuration);

            var sut = fixture.Create<UserPublishingService>();

            // Act
            await sut.SendUserAsync(user);

            // Assert
            publisher.Verify(x =>
                x.DispatchAsync(It.Is<UserMessageRequestModel>(u =>
                    u == user &&
                    u.ApiKey == "test-api-key")),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task SendUserAsync_ThrowsWrappedException_WhenDispatchFails(
            UserMessageRequestModel user,
            [Frozen] Mock<IUserPublisher> publisher,
            UserPublishingService sut)
                {
            // Arrange
            publisher
                .Setup(x => x.DispatchAsync(It.IsAny<UserMessageRequestModel>()))
                .ThrowsAsync(new Exception("fail"));

            // Act
            Func<Task> act = () => sut.SendUserAsync(user);

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();

            exception.Which.Message.Should()
                .Contain("An error occurred while sending the account confirmation message");

            exception.Which.InnerException.Should().NotBeNull();
        }
    }
}
