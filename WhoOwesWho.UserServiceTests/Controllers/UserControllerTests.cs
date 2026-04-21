using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Controllers;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Controllers
{
    public class UserControllerTests
    {
        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsOk_WhenSuccessful(
            SignUpRequestModel request,
            UserModel response,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock
)
        {
            // Arrange
            serviceMock
                .Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync(response);

            var sut = new UserController(
                serviceMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object
            );

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, ok.Value);
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsBadRequest_OnException(
            SignUpRequestModel request,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock)
        {
            // Arrange
            serviceMock
                .Setup(x => x.CreateUserAsync(request))
                .ThrowsAsync(new Exception("error"));

            var sut = new UserController(
               serviceMock.Object,
               validationMock.Object,
               recoveryMock.Object,
               resetMock.Object,
               changeMock.Object,
               securityMock.Object,
               commandMock.Object,
               lookupMock.Object,
               publishingMock.Object
           );

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("error", bad.Value);
        }

        [Theory, AutoMoqData]
        public async Task GetUnautorizedUserByEmailAddressAsync_ReturnsUser(
            string protectedEmail,
            string unprotectedEmail,
            UserModel user,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock)
        {
            // Arrange
            // Arrange
            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(unprotectedEmail);

            lookupMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            var sut = new UserController(
                serviceMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object
            );

            // Act
            var result = await sut.GetUnautorizedUserByEmailAddressAsync(protectedEmail, true);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeSameAs(user);
        }

        [Theory, AutoMoqData]
        public async Task Update_SetsId_AndCallsService(
            string protectedId,
            string unprotectedId,
            UserUpdateRequestModel request,
            UserModel response,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock)
        {
            // Arrange
            var guid = Guid.NewGuid();
            unprotectedId = guid.ToString();

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(unprotectedId);

            commandMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync(response);

            var sut = new UserController(
                serviceMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object
            );

            // Act
            var result = await sut.UpdateUserAsync(protectedId, request);

            // Assert
            commandMock.Verify(x =>
                x.UpdateUserAsync(It.Is<UserUpdateRequestModel>(r => r.Id == guid)),
                Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory, AutoMoqData]
        public async Task VerifyEmailAddress_Publishes_WhenSuccess(
            VerificationRequestModel request,
            string unprotectedEmail,
            UserModel response,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock)
        {
            // Arrange
            response.Success = true;

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(unprotectedEmail);

            validationMock
                .Setup(x => x.VerifyUserEmailAddress(unprotectedEmail))
                .ReturnsAsync(response);

            var sut = new UserController(
                serviceMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object
            );

            // Act
            var result = await sut.VerifyEmailAddressAsync(request);

            // Assert
            publishingMock.Verify(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()), Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory, AutoMoqData]
        public async Task VerifyEmailAddress_DoesNotPublish_WhenFailed(
            VerificationRequestModel request,
            string unprotectedEmail,
            UserModel response,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock)
        {
            // Arrange
            response.Success = false;

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(unprotectedEmail);

            validationMock
                .Setup(x => x.VerifyUserEmailAddress(unprotectedEmail))
                .ReturnsAsync(response);

            var sut = new UserController(
                serviceMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object
            );

            // Act
            await sut.VerifyEmailAddressAsync(request);

            // Assert
            publishingMock.Verify(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task ChangePassword_Publishes_WhenSuccess(
            ChangePasswordRequestModel request,
            ChangePasswordResponseModel response,
            UserModel user,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IPasswordRecoveryService> recoveryMock,
            [Frozen] Mock<IResetPasswordService> resetMock,
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingServicee> publishingMock)
        {
            // Arrange
            response.Success = true;

            changeMock
                .Setup(x => x.ChangePasswordAsync(request))
                .ReturnsAsync(response);

            lookupMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress!, true))
                .ReturnsAsync(user);

            var sut = new UserController(
                serviceMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object
            );

            // Act
            var result = await sut.ChangePasswordAsync(request);

            // Assert
            publishingMock.Verify(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()), Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
