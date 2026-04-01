using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using Xunit;

namespace WhoOwesWho.AuthorizationServiceTests.Repositories
{
    public class AuthorizationCacheRepositoryTests
    {
        [Theory, AutoMoqData]
        public async Task GetUserAsync_ReturnsNull_WhenNoValue(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut)
        {
            // Arrange
            db.Setup(x => x.StringGetAsync("user:test@test.com", It.IsAny<CommandFlags>()))
              .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await sut.GetUserAsync("test@test.com");

            // Assert
            result.Should().BeNull();
        }

        [Theory, AutoMoqData]
        public async Task GetUserAsync_ReturnsUser_WhenValueExists(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut,
            UserMessageResponseModel user)
        {
            // Arrange
            var json = JsonSerializer.Serialize(user);

            db.Setup(x => x.StringGetAsync("user:test@test.com", It.IsAny<CommandFlags>()))
              .ReturnsAsync(json);

            // Act
            var result = await sut.GetUserAsync("test@test.com");

            // Assert
            result.Should().BeEquivalentTo(user);
        }

        [Theory, AutoMoqData]
        public async Task GetUserByIdAsync_ReturnsNull_WhenNoValue(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut)
        {
            // Arrange
            db.Setup(x => x.StringGetAsync("user:123", It.IsAny<CommandFlags>()))
              .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await sut.GetUserByIdAsync("123");

            // Assert
            result.Should().BeNull();
        }

        [Theory, AutoMoqData]
        public async Task GetUserByIdAsync_ReturnsUser_WhenValueExists(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut,
            UserMessageResponseModel user)
        {
            // Arrange
            var json = JsonSerializer.Serialize(user);

            db.Setup(x => x.StringGetAsync("user:123", It.IsAny<CommandFlags>()))
              .ReturnsAsync(json);

            // Act
            var result = await sut.GetUserByIdAsync("123");

            // Assert
            result.Should().BeEquivalentTo(user);
        }

        [Theory, AutoMoqData]
        public async Task SaveUserAsync_SetsValue_ByEmail(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut,
            UserMessageRequestModel user)
        {
            // Arrange
            user.EmailAddress = "test@test.com";

            // Act
            await sut.SaveUserAsync(user);

            // Assert
            db.Verify(x => x.StringSetAsync(
                $"user:{user.EmailAddress}",
                It.IsAny<RedisValue>(),
                It.IsAny<Expiration>(),
                It.IsAny<ValueCondition>(),
                CommandFlags.None),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task SaveUserAsync_SetsValue_ById(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut,
            UserMessageRequestModel user)
        {
            // Act
            await sut.SaveUserAsync(user);

            // Assert
            db.Verify(x => x.StringSetAsync(
                $"user:{user.Id}",
                It.IsAny<RedisValue>(),
                It.IsAny<Expiration>(),
                It.IsAny<ValueCondition>(),
                CommandFlags.None),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task SaveUserAsync_StoresSerializedUser(
            [Frozen] Mock<IDatabase> db,
            AuthorizationCacheRepository sut,
            UserMessageRequestModel user)
        {
            // Arrange
            RedisValue? captured = null;

            db.Setup(x => x.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<Expiration>(),
                    It.IsAny<ValueCondition>(),
                    It.IsAny<CommandFlags>()))
              .Callback<RedisKey, RedisValue, Expiration, ValueCondition, CommandFlags>((_, value, _, _, _) =>
              {
                  captured = value;
              })
              .ReturnsAsync(true);

            // Act
            await sut.SaveUserAsync(user);

            // Assert
            captured.Should().NotBeNull();

            var deserialized = JsonSerializer.Deserialize<UserMessageRequestModel>(captured!);
            deserialized.Should().BeEquivalentTo(user);
        }
    }


}
