using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{
    public class UserValidationServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ValidatePasswordAsync_ReturnsValid_WhenPasswordIsValid(
             [Frozen] Mock<IConfiguration> configurationMock,
            UserValidationService sut)
        {
            //Arrange
            configurationMock
                .Setup(x => x["Password:Format:LenghtRequired"])
                .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var password = "Valid123";

            //Act
            var result = await sut.ValidatePasswordAsync(password);

            //Assert
            result.isValid.Should().BeTrue();
            result.errorMessage.Should().BeEmpty();
        }

        [Theory, AutoMoqData]
        public async Task ValidatePasswordAsync_ReturnsError_WhenPasswordIsInvalid(
            [Frozen] Mock<IConfiguration> configurationMock,
            UserValidationService sut)
        {
            //Arrange
            configurationMock
                .Setup(x => x["Password:Format:LenghtRequired"])
                .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var password = "x";

            //Act
            var result = await sut.ValidatePasswordAsync(password);

            //Assert
            result.isValid.Should().BeFalse();
            result.errorMessage.Should().NotBeEmpty();
        }

        [Theory, AutoMoqData]
        public async Task ValidateEmailAsync_ReturnsError_WhenEmailIsInvalid(
            UserValidationService sut)
        {
            //Act
            var result = await sut.ValidateEmailAsync(string.Empty, false);

            //Assert
            result.isValid.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateEmailAsync_ReturnsError_WhenEmailAlreadyExists(
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserValidationService sut)
        {
            //Arrange
            var email = "test@test.com";

            queryRepository
                .Setup(x => x.GetUserEmailExists(email))
                .ReturnsAsync(true);

            //Act
            var result = await sut.ValidateEmailAsync(email, false);

            //Assert
            result.isValid.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateEmailAsync_ReturnsValid_WhenEmailDoesNotExist(
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserValidationService sut)
        {
            //Arrange
            var email = "test@test.com";

            queryRepository
                .Setup(x => x.GetUserEmailExists(email))
                .ReturnsAsync(false);

            //Act
            var result = await sut.ValidateEmailAsync(email, false);

            //Assert
            result.isValid.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task ValidateEmailAsync_ReturnsError_WhenEmailDoesNotExist_ButShouldExist(
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserValidationService sut)
        {
            //Arrange
            var email = "test@test.com";

            queryRepository
                .Setup(x => x.GetUserEmailExists(email))
                .ReturnsAsync(false);

            //Act
            var result = await sut.ValidateEmailAsync(email, true);

            //Assert
            result.isValid.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateEmailAsync_ReturnsValid_WhenEmailExists_AndShouldExist(
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserValidationService sut)
        {
            //Arrange
            var email = "test@test.com";

            queryRepository
                .Setup(x => x.GetUserEmailExists(email))
                .ReturnsAsync(true);

            //Act
            var result = await sut.ValidateEmailAsync(email, true);

            //Assert
            result.isValid.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task VerifyUserEmailAddress_ReturnsNull_WhenUserNotFound(
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserValidationService sut)
        {
            //Arrange
            var email = "test@test.com";

            queryRepository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(email, true))
                .ReturnsAsync((UserModel?)null);

            //Act
            var result = await sut.VerifyUserEmailAddressAsync(email);

            //Assert
            result.Should().BeNull();
        }

        [Theory, AutoMoqData]
        public async Task VerifyUserEmailAddress_UpdatesUser_WhenUserExists(
            UserModel user,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            [Frozen] Mock<IUserMutationRepository> mutationRepository,
            UserValidationService sut)
        {
            //Arrange
            var email = "test@test.com";

            queryRepository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(email, true))
                .ReturnsAsync(user);

            mutationRepository
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(user);

            //Act
            var result = await sut.VerifyUserEmailAddressAsync(email);

            //Assert
            result.Should().NotBeNull();
            result!.EmailAddressVerified.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task VerifyUpdate_ReturnsSuccess_WhenEventIdIsNull(
            UserUpdateRequestModel request,
            UserValidationService sut)
        {
            request.EventId = null;

            var result = await sut.VerifyUpdateAsync(request);

            result.Success.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task VerifyUpdate_ReturnsFailure_WhenAnotherAdminExists(
            UserUpdateRequestModel request,
            EventMessageResponseModel evt,
            UserMessageResponseModel adminUser,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserCacheRepository> cacheRepository,
            UserValidationService sut)
        {
            //Arrange
            request.Admin = true;
            request.EventId = "event";
            request.ProtectedId = "user";

            adminUser.Admin = true;

            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync(Guid.NewGuid().ToString());

            evt.UserIds = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

            cacheRepository
                .Setup(x => x.GetActiveEventByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(evt);

            cacheRepository
                .Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(adminUser);

            //Act
            var result = await sut.VerifyUpdateAsync(request);

            //Assert
            result.Success.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task VerifyUpdate_ReturnsNoAdminWarning_WhenNoAdminExists(
            UserUpdateRequestModel request,
            EventMessageResponseModel evt,
            UserMessageResponseModel user,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserCacheRepository> cacheRepository,
            UserValidationService sut)
                {
            request.Admin = false;
            request.EventId = "event";
            request.ProtectedId = "user";

            //Arrange
            user.Admin = false;

            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync(Guid.NewGuid().ToString());

            evt.UserIds = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

            cacheRepository
                .Setup(x => x.GetActiveEventByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(evt);

            cacheRepository
                .Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            //Act
            var result = await sut.VerifyUpdateAsync(request);

            //Assert
            result.Success.Should().BeTrue();
            result.NoAdmin.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task VerifyUpdate_ReturnsFailure_WhenExceptionOccurs(
            UserUpdateRequestModel request,
            [Frozen] Mock<IUserSecurityService> securityService,
            UserValidationService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ThrowsAsync(new Exception());

            var result = await sut.VerifyUpdateAsync(request);

            result.Success.Should().BeFalse();
        }
    }
}
