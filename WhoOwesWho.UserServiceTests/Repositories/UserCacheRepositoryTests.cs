using AutoFixture.Xunit2;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Repositories;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Repositories
{
    public class UserCacheRepositoryTests
    {
        [Theory, AutoMoqData]
        public async Task GetUserByIdAsync_ReturnsNull_WhenRedisHasNoValue(
            string userId,
            [Frozen] Mock<IDatabase> dbMock,
            IUserCacheRepository.UserCacheRepository sut)
        {
            // Arrange
            dbMock
                .Setup(x => x.StringGetAsync($"user:{userId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await sut.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Theory, AutoMoqData]
        public async Task GetUserByIdAsync_ReturnsUser_WhenValueExists(
            string userId,
            UserMessageResponseModel user,
            [Frozen] Mock<IDatabase> dbMock,
            IUserCacheRepository.UserCacheRepository sut)
        {
            // Arrange
            var json = JsonSerializer.Serialize(user);

            dbMock
                .Setup(x => x.StringGetAsync($"user:{userId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(json);

            // Act
            var result = await sut.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Theory, AutoMoqData]
        public async Task GetActiveEventByIdAsync_ReturnsNull_WhenRedisHasNoValue(
            string eventId,
            [Frozen] Mock<IDatabase> dbMock,
            IUserCacheRepository.UserCacheRepository sut)
        {
            // Arrange
            dbMock
                .Setup(x => x.StringGetAsync($"activeevent:{eventId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await sut.GetActiveEventByIdAsync(eventId);

            // Assert
            Assert.Null(result);
        }

        [Theory, AutoMoqData]
        public async Task GetActiveEventByIdAsync_ReturnsNull_WhenEventIsSettled(
            string eventId,
            EventMessageResponseModel eventModel,
            [Frozen] Mock<IDatabase> dbMock,
            IUserCacheRepository.UserCacheRepository sut)
        {
            // Arrange
            eventModel.Settled = true;
            var json = JsonSerializer.Serialize(eventModel);

            dbMock
                .Setup(x => x.StringGetAsync($"activeevent:{eventId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(json);

            // Act
            var result = await sut.GetActiveEventByIdAsync(eventId);

            // Assert
            Assert.Null(result);
        }

        [Theory, AutoMoqData]
        public async Task GetActiveEventByIdAsync_ReturnsEvent_WhenNotSettled(
            string eventId,
            EventMessageResponseModel eventModel,
            [Frozen] Mock<IDatabase> dbMock,
            IUserCacheRepository.UserCacheRepository sut)
        {
            // Arrange
            eventModel.Settled = false;
            var json = JsonSerializer.Serialize(eventModel);

            dbMock
                .Setup(x => x.StringGetAsync($"activeevent:{eventId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(json);

            // Act
            var result = await sut.GetActiveEventByIdAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(eventModel.Id, result!.Id);
        }
    }
}
