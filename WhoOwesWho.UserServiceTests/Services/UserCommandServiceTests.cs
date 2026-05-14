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
    public class UserCommandServiceTests
    {
        [Theory, AutoData]
        public async Task CreateUserAsync_ReturnsError_WhenUserCreationFails(
            UserModel request,
            string host)
        {
            // Arrange
            var mutationRepositoryMock = new Mock<IUserMutationRepository>();

            mutationRepositoryMock
                .Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateUserCommandService(
                mutationRepositoryMock: mutationRepositoryMock);

            // Act
            var result = await sut.CreateUserAsync(request, host);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.UserCreationErrorMessages.UserLoadingUnsuccessful);
        }

        [Theory, AutoData]
        public async Task CreateUserAsync_ReturnsUser_WhenSuccessful(
            UserModel request,
            UserModel createdUser,
            UserModel loadedUser,
            string host)
        {
            // Arrange
            createdUser.EmailAddress = request.EmailAddress;

            var mutationRepositoryMock = new Mock<IUserMutationRepository>();
            var notificationServiceMock = new Mock<IUserNotificationService>();
            var publishingServiceMock = new Mock<IUserPublishingService>();
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            mutationRepositoryMock
                .Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync(createdUser);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(createdUser.EmailAddress, true))
                .ReturnsAsync(loadedUser);

            publishingServiceMock
                .Setup(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()))
                .Returns(Task.CompletedTask);

            notificationServiceMock
                .Setup(x => x.SendAccountConfirmationMessage(
                    It.IsAny<UserMessageRequestModel>(),
                    host))
                .Returns(Task.CompletedTask);

            var sut = CreateUserCommandService(
                mutationRepositoryMock: mutationRepositoryMock,
                notificationServiceMock: notificationServiceMock,
                publishingServiceMock: publishingServiceMock,
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.CreateUserAsync(request, host);

            // Assert
            result.Should().Be(loadedUser);

            publishingServiceMock.Verify(
                x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()),
                Times.Once);

            notificationServiceMock.Verify(
                x => x.SendAccountConfirmationMessage(
                    It.IsAny<UserMessageRequestModel>(),
                    host),
                Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateUserAsync_ReturnsValidationMessage_WhenValidationFails(
            UserUpdateRequestModel request,
            UserModel user)
        {
            // Arrange
            request.IsPasswordUpdating = false;

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var validationServiceMock = new Mock<IUserValidationService>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id))
                .ReturnsAsync(user);

            validationServiceMock
                .Setup(x => x.ValidateUpdateAsync(request))
                .ReturnsAsync(new UpdateUserVerificationModel
                {
                    Success = false,
                    AdministratorNonExisting = false,
                    Message = Constants.UserUpdatingErrorMessages.AdministratorAlreadyExisting
                });

            var sut = CreateUserCommandService(
                queryRepositoryMock: queryRepositoryMock,
                validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.UpdateUserAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Message.Should()
                .Be(Constants.UserUpdatingErrorMessages.AdministratorAlreadyExisting);

            result.Success.Should().BeFalse();
        }

        [Theory, AutoData]
        public async Task UpdateUserAsync_ReturnsWarningMessage_WhenNoAdministratorExists(
            UserUpdateRequestModel request,
            UserModel existingUser,
            UserModel updatedUser)
        {
            // Arrange
            request.IsPasswordUpdating = false;

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var validationServiceMock = new Mock<IUserValidationService>();
            var mutationRepositoryMock = new Mock<IUserMutationRepository>();
            var publishingServiceMock = new Mock<IUserPublishingService>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id))
                .ReturnsAsync(existingUser);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id, true))
                .ReturnsAsync(existingUser);

            validationServiceMock
                .Setup(x => x.ValidateUpdateAsync(request))
                .ReturnsAsync(new UpdateUserVerificationModel
                {
                    Success = false,
                    AdministratorNonExisting = true,
                    Message = Constants.UserUpdatingErrorMessages.NoAdministratorExisting
                });

            mutationRepositoryMock
                .Setup(x => x.UpdateUserAsync(existingUser))
                .ReturnsAsync(updatedUser);

            publishingServiceMock
                .Setup(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()))
                .Returns(Task.CompletedTask);

            var sut = CreateUserCommandService(
                queryRepositoryMock: queryRepositoryMock,
                validationServiceMock: validationServiceMock,
                mutationRepositoryMock: mutationRepositoryMock,
                publishingServiceMock: publishingServiceMock);

            // Act
            var result = await sut.UpdateUserAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeFalse();

            result.Message.Should()
                .Be(Constants.UserUpdatingErrorMessages.NoAdministratorExisting);

            publishingServiceMock.Verify(
                x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()),
                Times.Exactly(2));
        }

        [Theory, AutoData]
        public async Task UpdateUserAsync_ReturnsUpdatedUser_WhenSuccessful(
            UserUpdateRequestModel request,
            UserModel existingUser,
            UserModel updatedUser)
        {
            // Arrange
            request.IsPasswordUpdating = false;

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var validationServiceMock = new Mock<IUserValidationService>();
            var mutationRepositoryMock = new Mock<IUserMutationRepository>();
            var publishingServiceMock = new Mock<IUserPublishingService>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id))
                .ReturnsAsync(existingUser);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id, true))
                .ReturnsAsync(existingUser);

            validationServiceMock
                .Setup(x => x.ValidateUpdateAsync(request))
                .ReturnsAsync(new UpdateUserVerificationModel
                {
                    Success = true,
                    AdministratorNonExisting = true
                });

            mutationRepositoryMock
                .Setup(x => x.UpdateUserAsync(existingUser))
                .ReturnsAsync(updatedUser);

            publishingServiceMock
                .Setup(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()))
                .Returns(Task.CompletedTask);

            var sut = CreateUserCommandService(
                queryRepositoryMock: queryRepositoryMock,
                validationServiceMock: validationServiceMock,
                mutationRepositoryMock: mutationRepositoryMock,
                publishingServiceMock: publishingServiceMock);

            // Act
            var result = await sut.UpdateUserAsync(request);

            // Assert
            result.Should().NotBeNull();

            result!.Success.Should().BeTrue();

            result.Message.Should()
                .Be(Constants.UserUpdatingErrorMessages.UpdateSucceeded);

            publishingServiceMock.Verify(
                x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()),
                Times.Exactly(2));
        }

        [Theory, AutoData]
        public async Task UpdateUserAsync_UpdatesPassword_WhenPasswordUpdating(
            UserUpdateRequestModel request,
            UserModel existingUser,
            UserModel updatedUser)
        {
            // Arrange
            request.IsPasswordUpdating = true;
            request.Password = "NewPassword";

            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var mutationRepositoryMock = new Mock<IUserMutationRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id))
                .ReturnsAsync(existingUser);

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByIdAsync(request.Id, true))
                .ReturnsAsync(existingUser);

            mutationRepositoryMock
                .Setup(x => x.UpdateUserAsync(existingUser))
                .ReturnsAsync(updatedUser);

            var sut = CreateUserCommandService(
                queryRepositoryMock: queryRepositoryMock,
                mutationRepositoryMock: mutationRepositoryMock);

            // Act
            await sut.UpdateUserAsync(request);

            // Assert
            existingUser.Password.Should().Be(request.Password);
        }

        private static UserCommandService CreateUserCommandService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IUserNotificationService>? notificationServiceMock = null,
            Mock<IUserQueryRepository>? queryRepositoryMock = null,
            Mock<IUserMutationRepository>? mutationRepositoryMock = null,
            Mock<IUserPublishingService>? publishingServiceMock = null,
            Mock<IUserValidationService>? validationServiceMock = null)
        {
            configurationMock ??= new();
            notificationServiceMock ??= new();
            queryRepositoryMock ??= new();
            mutationRepositoryMock ??= new();
            publishingServiceMock ??= new();
            validationServiceMock ??= new();

            return new UserCommandService(
                configurationMock.Object,
                notificationServiceMock.Object,
                queryRepositoryMock.Object,
                mutationRepositoryMock.Object,
                publishingServiceMock.Object,
                validationServiceMock.Object);
        }
    }
}