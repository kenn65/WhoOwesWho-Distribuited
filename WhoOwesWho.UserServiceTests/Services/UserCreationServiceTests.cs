using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{
    public class UserCreationServiceTests
    {
        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsError_WhenFullNameIsMissing(
            SignUpRequestModel request,
            [Frozen] Mock<IUserSecurityService> securityService,
            UserCreationService sut)
        {
            request.Entity!.FullName = "";

            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com");

            var result = await sut.CreateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Full name is required.");
        }


        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsError_WhenEmailValidationFails(
            SignUpRequestModel request,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserValidationService> validationService,
            UserCreationService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com");

            validationService
                .Setup(x => x.ValidateEmailAsync("email@test.com", false))
                .ReturnsAsync((false, "Invalid email"));

            var result = await sut.CreateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid email");
        }


        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsError_WhenPasswordValidationFails(
            SignUpRequestModel request,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserValidationService> validationService,
            UserCreationService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com");

            validationService
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), false))
                .ReturnsAsync((true, ""));

            validationService
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((false, "Invalid password"));

            var result = await sut.CreateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid password");
        }


        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsSuccess_WhenUserIsCreated(
            SignUpRequestModel request,
            UserModel createdUser,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserValidationService> validationService,
            [Frozen] Mock<IUserCommandService> commandService,
            UserCreationService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com");

            validationService
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), false))
                .ReturnsAsync((true, ""));

            validationService
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((true, ""));

            commandService
                .Setup(x => x.CreateUserAsync(request.Entity!, request.Host!))
                .ReturnsAsync(createdUser);

            var result = await sut.CreateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Message.Should().Contain("Sign up successful");
        }


        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsError_WhenUserCreationFails(
            SignUpRequestModel request,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserValidationService> validationService,
            [Frozen] Mock<IUserCommandService> commandService,
            UserCreationService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("email@test.com");

            validationService
                .Setup(x => x.ValidateEmailAsync(It.IsAny<string>(), false))
                .ReturnsAsync((true, ""));

            validationService
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((true, ""));

            commandService
                .Setup(x => x.CreateUserAsync(request.Entity!, request.Host!))
                .ReturnsAsync((UserModel?)null);

            var result = await sut.CreateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred, please try again.");
        }


        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsError_WhenExceptionOccurs(
            SignUpRequestModel request,
            [Frozen] Mock<IUserSecurityService> securityService,
            UserCreationService sut)
        {
            securityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());

            var result = await sut.CreateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred, please try again.");
        }
    }
}