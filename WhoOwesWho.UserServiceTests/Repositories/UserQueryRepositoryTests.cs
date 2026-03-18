using FluentAssertions;
using WhoOwesWho.UserService.EfCore.DataModels;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserServiceTests.Repositories.Context;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Repositories
{
    public class UserQueryRepositoryTests
    {
        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            context.Users.AddRange(
                new Users
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = "a@test.com",
                    FullName = "fullname",
                    Password = "password",
                    MobilePhoneNumber = "1234567890",
                    EmailAddressVerified = true,
                    Admin = false,
                },
                new Users
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = "b@test.com",
                    FullName = "fullname1",
                    Password = "password1",
                    MobilePhoneNumber = "12345678901",
                    EmailAddressVerified = true,
                    Admin = false,
                });
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetAllUsersAsync();

            //Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSingleUserByEmailAddressAsync_ShouldReturnUser()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var user = new Users
            {
                Id = Guid.NewGuid(),
                EmailAddress = "test@test.com",
                FullName = "fullname",
                Password = "secret",
                MobilePhoneNumber = "1234567890",
                EmailAddressVerified = true,
                Admin = false,
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetSingleUserByEmailAddressAsync("test@test.com", true);

            //Assert
            result.Should().NotBeNull();
            result!.EmailAddress.Should().Be("test@test.com");
            result.Password.Should().Be("secret");
        }

        [Fact]
        public async Task GetSingleUserByEmailAddressAsync_ShouldHidePassword_WhenNotComplete()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            context.Users.Add(new Users
            {
                Id = Guid.NewGuid(),
                EmailAddress = "test@test.com",
                FullName = "fullname",
                Password = "secret",
                MobilePhoneNumber = "1234567890",
                EmailAddressVerified = true,
                Admin = false,
            });
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetSingleUserByEmailAddressAsync("test@test.com", false);

            //Assert
            result.Should().NotBeNull();
            result!.Password.Should().BeNull();
        }

        [Fact]
        public async Task GetSingleUserByEmailAddressAsync_ShouldReturnNull_WhenNotFound()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetSingleUserByEmailAddressAsync("missing@test.com");

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSingleUserByIdAsync_ShouldReturnUser()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var id = Guid.NewGuid();
            context.Users.Add(new Users
            {
                Id = id,
                EmailAddress = "test@test.com",
                FullName = "fullname",
                Password = "secret",
                MobilePhoneNumber = "1234567890",
                EmailAddressVerified = true,
                Admin = false,
            });
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetSingleUserByIdAsync(id, true);

            //Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetSingleUserByIdAsync_ShouldHidePassword_WhenNotComplete()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var id = Guid.NewGuid();
            context.Users.Add(new Users
            {
                Id = id,
                EmailAddress = "test@test.com",
                FullName = "fullname",
                Password = "secret",
                MobilePhoneNumber = "1234567890",
                EmailAddressVerified = true,
                Admin = false,
            });
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetSingleUserByIdAsync(id, false);

            //Assert
            result.Should().NotBeNull();
            result!.Password.Should().BeNull();
        }

        [Fact]
        public async Task GetSingleUserByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetSingleUserByIdAsync(Guid.NewGuid());

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserEmailExists_ShouldReturnTrue_WhenExists()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            context.Users.Add(new Users
            {
                Id = Guid.NewGuid(),
                EmailAddress = "test@test.com",
                FullName = "fullname",
                Password = "secret",
                MobilePhoneNumber = "1234567890",
                EmailAddressVerified = true,
                Admin = false,
            });
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetUserEmailExists("test@test.com");

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserEmailExists_ShouldReturnFalse_WhenNotExists()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetUserEmailExists("missing@test.com");

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetForgotPasswordTokenAsync_ShouldReturnToken()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var userId = Guid.NewGuid();
            context.ForgotPasswords.Add(new ForgotPassword
            {
                UserId = userId,
                ExpirationTime = DateTime.UtcNow.Ticks,
                ForgotPasswordToken = "token"
            });
            await context.SaveChangesAsync();
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetForgotPasswordTokenAsync(userId);

            //Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetForgotPasswordTokenAsync_ShouldReturnNull_WhenNotFound()
        {
            //Arrange
            using var context = DbContextFactory.CreateContext(out var connection);
            var sut = new UserQueryRepository(context);

            //Act
            var result = await sut.GetForgotPasswordTokenAsync(Guid.NewGuid());

            //Asssert
            result.Should().BeNull();
        }
    }
}
