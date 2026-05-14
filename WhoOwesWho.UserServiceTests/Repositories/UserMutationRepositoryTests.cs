using FluentAssertions;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.EfCore.DataModels;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserServiceTests.Repositories.Context;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Repositories
{
    public class UserMutationRepositoryTests
    {
        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserMutationRepository(context);

            var user = new UserModel
            {
                Id = Guid.NewGuid(),
                EmailAddress = "test@test.com",
                FullName = "Test User",
                MobilePhoneNumber = "1234567890",
                Password = "ælkjadslkjsldfjl"
            };

            //Act
            var result = await sut.CreateUserAsync(user);

            //Assert
            result.Should().NotBeNull();
            result!.EmailAddress.Should().Be(user.EmailAddress);
            context.Users.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenEntityIsNull()
        {
            // Arrange
            var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserMutationRepository(context);

            // Act
            Func<Task> act = async () =>
                await sut.CreateUserAsync(null!);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);

            var userId = Guid.NewGuid();

            context.Users.Add(new Users
            {
                Id = userId,
                EmailAddress = "old@test.com",
                FullName = "Old Name",
                MobilePhoneNumber = "1234567890",
                Password = "ælkjadslkjsldfjl"
            });

            await context.SaveChangesAsync();

            var sut = new UserMutationRepository(context);

            var update = new UserModel
            {
                Id = userId,
                EmailAddress = "new@test.com",
                FullName = "New Name",
                MobilePhoneNumber = "1234567890",
                Password = "ælkjadslkjsldfjl",
                Admin = false,
                EmailAddressVerified = true
            };

            //Act
            var result = await sut.UpdateUserAsync(update);

            //Asssert
            result.Should().NotBeNull();
            result!.EmailAddress.Should().Be("new@test.com");
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenEntityIsNull()
        {
            // Arrange
            var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserMutationRepository(context);

            // Act
            Func<Task> act = async () =>
                await sut.UpdateUserAsync((UserModel)null!);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>();
        }

        [Fact]
        public async Task CreateForgotPasswordTokenAsync_ShouldCreateToken()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserMutationRepository(context);

            var model = new ForgotPasswordTokenModel
            {
                UserId = Guid.NewGuid(),
                ForgotPasswordToken = "token",
                ExpirationTime = DateTime.UtcNow.Ticks
            };

            //Act
            var result = await sut.CreateForgotPasswordTokenAsync(model);

            //Assert
            result.Should().BeTrue();
            context.ForgotPasswords.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateForgotPasswordTokenAsync_ShouldThrow_WhenModelIsNull()
        {
            // Arrange
            var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserMutationRepository(context);

            // Act
            Func<Task> act = async () =>
                await sut.CreateForgotPasswordTokenAsync(null!);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>();
        }

        [Fact]
        public async Task DeleteForgotPasswordTokenAsync_ShouldDeleteToken()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);

            var userId = Guid.NewGuid();

            context.ForgotPasswords.Add(new ForgotPassword
            {
                UserId = userId,
                ForgotPasswordToken = "token",
                ExpirationTime= DateTime.UtcNow.Ticks
                
            });

            await context.SaveChangesAsync();

            var sut = new UserMutationRepository(context);

            var result = await sut.DeleteForgotPasswordTokenAsync(userId);

            result.Should().BeTrue();
            context.ForgotPasswords.Count().Should().Be(0);
        }

        [Fact]
        public async Task DeleteForgotPasswordTokenAsync_ShouldThrow_WhenExceptionOccurs()
        {
            // Arrange
            var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserMutationRepository(context);

            context.Dispose();

            // Act
            Func<Task> act = async () =>
                await sut.DeleteForgotPasswordTokenAsync(Guid.NewGuid());

            // Assert
            await act.Should()
                .ThrowAsync<ObjectDisposedException>();
        }
    }
}
