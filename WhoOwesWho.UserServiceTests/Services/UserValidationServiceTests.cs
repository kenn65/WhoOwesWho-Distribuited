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
    public class UserValidationServiceTests
    {
        [Theory, AutoData]
        public async Task VerifyUserEmailAddressAsync_ReturnsUpdatedUser_WhenSuccessful(
            string email,
            UserModel user,
            UserModel updatedUser)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();
            var mutationRepositoryMock = new Mock<IUserMutationRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(email, true))
                .ReturnsAsync(user);

            mutationRepositoryMock
                .Setup(x => x.UpdateUserAsync(user))
                .ReturnsAsync(updatedUser);

            var sut = CreateUserValidationService(
                queryRepositoryMock: queryRepositoryMock,
                mutationRepositoryMock: mutationRepositoryMock);

            // Act
            var result = await sut.VerifyUserEmailAddressAsync(email);

            // Assert
            result.Should().Be(updatedUser);
            user.EmailAddressVerified.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task VerifyUserEmailAddressAsync_Throws_WhenRepositoryFails(
            string email)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(email, true))
                .ThrowsAsync(new Exception(Constants.UserCreationErrorMessages.UserLoadingUnsuccessful));

            var sut = CreateUserValidationService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            Func<Task> act = async () =>
                await sut.VerifyUserEmailAddressAsync(email);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.UserCreationErrorMessages.UserLoadingUnsuccessful);
        }

        [Theory, AutoData]
        public async Task ValidateUpdateAsync_ReturnsVerificationModel_WhenSuccessful(
            UserUpdateRequestModel request,
            UpdateUserVerificationModel verificationModel)
        {
            // Arrange
            var updateValidationMock =
                new Mock<IUserUpdateValidationService>();

            updateValidationMock
                .Setup(x => x.ValidateUpdateAsync(request))
                .ReturnsAsync(verificationModel);

            var sut = CreateUserValidationService(
                updateValidationMock: updateValidationMock);

            // Act
            var result = await sut.ValidateUpdateAsync(request);

            // Assert
            result.Should().Be(verificationModel);
        }

        [Theory, AutoData]
        public async Task ValidateUpdateAsync_Throws_WhenValidationFails(
            UserUpdateRequestModel request)
        {
            // Arrange
            var updateValidationMock =
                new Mock<IUserUpdateValidationService>();

            updateValidationMock
                .Setup(x => x.ValidateUpdateAsync(request))
                .ThrowsAsync(new Exception(Constants.GlobalErrorMessages.UnexpectedError));

            var sut = CreateUserValidationService(
                updateValidationMock: updateValidationMock);

            // Act
            Func<Task> act = async () =>
                await sut.ValidateUpdateAsync(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task IsFullNameUnique_ReturnsTrue_WhenFullNameDoesNotExist(
            string fullName)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetUserFullNameExists(fullName))
                .ReturnsAsync(false);

            var sut = CreateUserValidationService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.IsFullNameUniqueAsync(fullName);

            // Assert
            result.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task IsFullNameUnique_ReturnsFalse_WhenFullNameExists(
            string fullName)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetUserFullNameExists(fullName))
                .ReturnsAsync(true);

            var sut = CreateUserValidationService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.IsFullNameUniqueAsync(fullName);

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoData]
        public async Task IsEmailAddressUnique_ReturnsTrue_WhenEmailDoesNotExist(
            string protectedEmail,
            string unprotectedEmail)
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetUserEmailExists(unprotectedEmail))
                .ReturnsAsync(false);

            var sut = CreateUserValidationService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.IsEmailAddressUniqueAsync(protectedEmail);

            // Assert
            result.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task IsEmailAddressUnique_ReturnsFalse_WhenEmailExists(
            string email
            )
        {
            // Arrange
            var queryRepositoryMock = new Mock<IUserQueryRepository>();

            queryRepositoryMock
                .Setup(x => x.GetUserEmailExists(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateUserValidationService(
                queryRepositoryMock: queryRepositoryMock);

            // Act
            var result = await sut.IsEmailAddressUniqueAsync(email);

            // Assert
            result.Should().BeFalse();
        }

        private static UserValidationService CreateUserValidationService(
            Mock<IConfiguration>? configurationMock = null,
            Mock<IUserQueryRepository>? queryRepositoryMock = null,
            Mock<IUserMutationRepository>? mutationRepositoryMock = null,
            Mock<IUserUpdateValidationService>? updateValidationMock = null)
        {
            configurationMock ??= new();
            queryRepositoryMock ??= new();
            mutationRepositoryMock ??= new();
            updateValidationMock ??= new();

            return new UserValidationService(
                configurationMock.Object,
                queryRepositoryMock.Object,
                mutationRepositoryMock.Object,
                updateValidationMock.Object);
        }
    }
}