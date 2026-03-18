using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{
    public class ChangePasswordServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ChangePassword_ShouldReturnError_WhenEmailInvalid(
            ChangePasswordRequestModel request,
            [Frozen] Mock<IUserValidationService> validation,
            ChangePasswordService service)
        {
            validation
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
                .ReturnsAsync((false, "Invalid email"));

            var result = await service.ChangePasswordAsync(request);


            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid email");
        }

        [Theory, AutoMoqData]
        public async Task ChangePassword_ShouldFail_WhenPasswordsDoNotMatch(
            ChangePasswordRequestModel request,
            [Frozen] Mock<IUserSecurityService> security,
            [Frozen] Mock<IUserValidationService> validation,
            ChangePasswordService service)
        {
            security.SetupSequence(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com")
                .ReturnsAsync("oldPass")
                .ReturnsAsync("newPass1")
                .ReturnsAsync("newPass2");

            validation.Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
                .ReturnsAsync((true, ""));

            validation.Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((true, ""));

            var result = await service.ChangePasswordAsync(request);

            result!.Success.Should().BeFalse();
            result.Message.Should().Be("The passwords does not match!");
        }

        [Theory, AutoMoqData]
        public async Task ChangePassword_ShouldFail_WhenUserNotFound(
            ChangePasswordRequestModel request,
            [Frozen] Mock<IUserQueryRepository> repository,
            [Frozen] Mock<IUserValidationService> validation,
            ChangePasswordService service)
        {
            repository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(It.IsAny<string>(), true))
                .ReturnsAsync((UserModel?)null);

            validation.Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
               .ReturnsAsync((true, ""));

            validation.Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((true, ""));

            var result = await service.ChangePasswordAsync(request);

            result!.Success.Should().BeFalse();
            result.Message.Should().Contain("User not found");
        }

        [Theory, AutoMoqData]
        public async Task ChangePassword_ShouldSucceed_WhenPasswordUpdated(
            ChangePasswordRequestModel request,
            [Frozen] Mock<IUserQueryRepository> repository,
            [Frozen] Mock<IUserValidationService> validation,
            [Frozen] Mock<IUserCommandService> commandService,
            [Frozen] Mock<IUserSecurityService> security,
            ChangePasswordService service)
        {
            var user = new UserModel { 
                Id = Guid.NewGuid(),
                Password = request.Password
            };

            validation.Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), true))
               .ReturnsAsync((true, ""));

            validation.Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((true, ""));

            repository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(It.IsAny<string>(), true))
                .ReturnsAsync(user);

            security.Setup(x => x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("protectedId");

            commandService
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync(user);

            var result = await service.ChangePasswordAsync(request);

            result!.Success.Should().BeTrue();
            result.Message.Should().Contain("Your password was successfully changed.");
        }
    }
}
