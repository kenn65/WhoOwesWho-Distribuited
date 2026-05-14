using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserService.Tests.Services
{
    public class ChangePasswordServiceTests
    {
       [Theory, AutoData]
        public async Task ChangePasswordAsync_ReturnsError_WhenNewPasswordsDoNotMatch(
            ChangePasswordRequestModel request)
        {
            // Arrange
            request.NewPassword1 = "Password1";
            request.NewPassword2 = "Password2";

            var sut = CreateChangePasswordService();
                                    
            // Act
            Func<Task> act = async () =>
                await sut.ChangePasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.ChangePasswordErrorMessages.NewPasswordsDoNotMatch);
        }
        

        [Theory, AutoData]
        public async Task ChangePasswordAsync_ReturnsError_WhenUserDoesNotExist(
            ChangePasswordRequestModel request)
        {
            // Arrange
            request.NewPassword1 = "Password1";
            request.NewPassword2 = "Password1";

            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateChangePasswordService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            Func<Task> act = async () =>
                await sut.ChangePasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.ChangePasswordErrorMessages.UserNotFound);
        }

        [Theory, AutoData]
        public async Task ChangePasswordAsync_ReturnsError_WhenExistingPasswordIsInvalid(
            ChangePasswordRequestModel request,
            UserModel user)
        {
            // Arrange
            request.Password = "WrongPassword";
            request.NewPassword1 = "NewPassword";
            request.NewPassword2 = "NewPassword";

            user.Password = "CorrectPassword";

            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync(user);

            var sut = CreateChangePasswordService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            Func<Task> act = async () =>
                await sut.ChangePasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.ChangePasswordErrorMessages.ExistingPasswordInvalid);
        }

        [Theory, AutoData]
        public async Task ChangePasswordAsync_ReturnsSuccess_WhenPasswordIsChanged(
            ChangePasswordRequestModel request,
            UserModel user,
            UserModel updatedUser,
            [Frozen] Mock<IUserSecurityService> securityMock)
        {
            // Arrange
            request.Password = "OldPassword";
            request.NewPassword1 = "NewPassword";
            request.NewPassword2 = "NewPassword";

            user.Password = "OldPassword";

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var commandServiceMock = new Mock<IUserCommandService>();

            securityMock
                .Setup(x => x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string value, bool _) => value);

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string value, bool _) => value);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync(user);

            commandServiceMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync(updatedUser);

            var sut = CreateChangePasswordService(
                queryRepositoryMock: queryRepositoryMock,
                commandServiceMock: commandServiceMock,
                userSecurityServiceMock: securityMock);

            // Act
            var result = await sut.ChangePasswordAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeTrue();

            result.Message.Should()
                .Be(Constants.ChangePasswordErrorMessages.SuccessfullyChanged);

            user.Password.Should().Be(request.NewPassword1);
        }

        [Theory, AutoData]
        public async Task ChangePasswordAsync_Throws_WhenUpdateFails(
            ChangePasswordRequestModel request,
            UserModel user,
            [Frozen] Mock<IUserSecurityService> securityMock)
        {
            // Arrange
            request.Password = "OldPassword";
            request.NewPassword1 = "NewPassword";
            request.NewPassword2 = "NewPassword";

            user.Password = "OldPassword";

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var commandServiceMock = new Mock<IUserCommandService>();


            securityMock
                .Setup(x => x.ProtectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string value, bool _) => value);

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string value, bool _) => value);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ReturnsAsync(user);

            commandServiceMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateChangePasswordService(
                queryRepositoryMock: queryRepositoryMock,
                commandServiceMock: commandServiceMock,
                userSecurityServiceMock: securityMock);

            // Act
            Func<Task> act = async () =>
                await sut.ChangePasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.UserCreationErrorMessages.UserLoadingUnsuccessful);
        }

        [Theory, AutoData]
        public async Task ChangePasswordAsync_Throws_WhenRepositoryThrows(
            ChangePasswordRequestModel request)
        {
            // Arrange
            request.NewPassword1 = "NewPassword";
            request.NewPassword2 = "NewPassword";

            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress, true))
                .ThrowsAsync(new Exception(Constants.GlobalErrorMessages.UnexpectedError));

            var sut = CreateChangePasswordService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            Func<Task> act = async () =>
                await sut.ChangePasswordAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.GlobalErrorMessages.UnexpectedError);
        }

        private static ChangePasswordService CreateChangePasswordService(
           Mock<IConfiguration>? configurationMock = null,
           Mock<IUserQueryRepository>? queryRepositoryMock = null,
           Mock<IUserCommandService>? commandServiceMock = null,
           Mock<IUserSecurityService>? userSecurityServiceMock = null)
        {
            configurationMock ??= new();
            queryRepositoryMock ??= new();
            commandServiceMock ??= new();
            userSecurityServiceMock ??= new();

            return new ChangePasswordService(
                configurationMock.Object,
                userSecurityServiceMock.Object,
                queryRepositoryMock.Object,
                commandServiceMock.Object
               );
        }
    }
}