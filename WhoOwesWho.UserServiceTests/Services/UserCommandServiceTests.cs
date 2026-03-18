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
    public class UserCommandServiceTests
    {
        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsError_WhenRepositoryReturnsNull(
            UserModel request,
            string host,
            [Frozen] Mock<IUserMutationRepository> mutationRepo,
        UserCommandService sut)
        {
            mutationRepo
                .Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync((UserModel?)null);

            var result = await sut.CreateUserAsync(request, host);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("An error occurred while creating the user. Please, try again.");
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_SendsConfirmation_AndReturnsUser(
           UserModel request,
           UserModel createdUser,
           UserModel queriedUser,
           string host,
           [Frozen] Mock<IUserMutationRepository> mutationRepo,
           [Frozen] Mock<IUserQueryRepository> queryRepo,
           [Frozen] Mock<IUserNotificationService> notificationService,
           UserCommandService sut)
        {
            mutationRepo
                .Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync(createdUser);

            queryRepo
                .Setup(x => x.GetSingleUserByEmailAddressAsync(createdUser.EmailAddress, true))
                .ReturnsAsync(queriedUser);

            var result = await sut.CreateUserAsync(request, host);

            notificationService.Verify(
                x => x.SendAccountConfirmationMessage(It.IsAny<UserMessageRequestModel>(), host),
                Times.Once);

            result.Should().Be(queriedUser);
        }

        [Theory, AutoMoqData]
        public async Task UpdateUserAsync_ReturnsError_WhenAdminAlreadyExists(
           UserUpdateRequestModel request,
           [Frozen] Mock<IUserValidationService> validationService,
           UserCommandService sut)
        {
            validationService
                .Setup(x => x.VerifyUpdate(request))
                .ReturnsAsync(new UpdateUserVerificationModel
                {
                    Success = false,
                    NoAdmin = false
                });

            var result = await sut.UpdateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("The event you have assigned to already has an administrator.");
        }

        [Theory, AutoMoqData]
        public async Task UpdateUserAsync_UpdatesProfileSuccessfully(
            UserUpdateRequestModel request,
            UserModel existingUser,
            UserModel updatedUser,
            Guid userId,
            [Frozen] Mock<IUserValidationService> validationService,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserQueryRepository> queryRepo,
            [Frozen] Mock<IUserMutationRepository> mutationRepo,
            [Frozen] Mock<IUserPublishingServicee> publishingService,
        UserCommandService sut)
        {
            request.IsPasswordUpdating = false;

            validationService
                .Setup(x => x.VerifyUpdate(request))
                .ReturnsAsync(new UpdateUserVerificationModel
                {
                    Success = true,
                    NoAdmin = false
                });

            securityService
                .Setup(x => x.UnprotectAsync(request.ProtectedId!, false))
                .ReturnsAsync(userId.ToString());

            queryRepo
                .Setup(x => x.GetSingleUserByIdAsync(userId, true))
                .ReturnsAsync(existingUser);

            mutationRepo
                .Setup(x => x.UpdateUserAsync(existingUser))
                .ReturnsAsync(updatedUser);

            var result = await sut.UpdateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Message.Should().Be("Profile updated successfully.");

            publishingService.Verify(
                x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task UpdateUserAsync_ReturnsWarning_WhenEventHasNoAdmin(
            UserUpdateRequestModel request,
            UserModel existingUser,
            UserModel updatedUser,
            Guid userId,
            [Frozen] Mock<IUserSecurityService> securityService,
            [Frozen] Mock<IUserQueryRepository> queryRepo,
            [Frozen] Mock<IUserMutationRepository> mutationRepo,
            [Frozen] Mock<IUserPublishingServicee> publishingService,
            UserCommandService sut)
        {
            request.IsPasswordUpdating = true;

            securityService
                .Setup(x => x.UnprotectAsync(request.ProtectedId!, false))
                .ReturnsAsync(userId.ToString());

            queryRepo
                .Setup(x => x.GetSingleUserByIdAsync(userId, true))
                .ReturnsAsync(existingUser);

            mutationRepo
                .Setup(x => x.UpdateUserAsync(existingUser))
                .ReturnsAsync(updatedUser);

            var result = await sut.UpdateUserAsync(request);

            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Message.Should().Contain("no administrator");

            publishingService.Verify(
                x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()),
                Times.Once);
        }

    }
}
