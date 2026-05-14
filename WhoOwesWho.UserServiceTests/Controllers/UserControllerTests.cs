using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Controllers;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Validators;
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
        [Frozen] Mock<IUserValidationService> validationServiceMock,
        [Frozen] Mock<IUserSecurityService> securityMock)
        {
            // Arrange
            request.Host = "localhost";
            request.Entity!.FullName = "John Doe";
            request.Entity.EmailAddress = "john@test.com";
            request.Entity.MobilePhoneNumber = "12345678";
            request.Entity.Password = "Someone44";

            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string value, bool _) => value);

            serviceMock
                .Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync(response);

            var sut = CreateUserController(creationMock: serviceMock, validationMock: validationServiceMock, securityMock: securityMock);

            // Act
            var result =
                await sut.CreateUserAsync(request);

            // Assert
            var ok =
                Assert.IsType<OkObjectResult>(result);

            Assert.Equal(response, ok.Value);
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsBadRequest_WhenFullNameNotUnique(
           SignUpRequestModel request,
           [Frozen] Mock<IUserValidationService> validationServiceMock)
        {
            // Arrange
            request.Entity!.FullName = "John Test";
            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(false);

            var sut = CreateUserController(validationMock: validationServiceMock);

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<UserModel>(badRequest.Value);
            Assert.False(response.Success);
            Assert.Equal(
                Constants.CredentialsErrorMessages.FullNameAlreadyExists,
                response.Message);
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsBadRequest_WhenEmailInvalid(
            SignUpRequestModel request,
            [Frozen] Mock<IUserValidationService> validationServiceMock)
        {
            // Arrange
            request.Entity!.EmailAddress = "Invalid email";

            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(true);

            var sut = CreateUserController(validationMock: validationServiceMock);

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<UserModel>(badRequest.Value);
            Assert.False(response.Success);
            Assert.Equal(
                Constants.CredentialsErrorMessages.EmailAddressInvalid,
                response.Message);
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsBadRequest_WhenEmailNotUnique(
           SignUpRequestModel request,
           [Frozen] Mock<IUserValidationService> validationServiceMock)
        {
            // Arrange
            request.Entity!.EmailAddress = "john@test.com";

            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(false);

            var sut = CreateUserController(validationMock: validationServiceMock);

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<UserModel>(badRequest.Value);
            Assert.False(response.Success);
            Assert.Equal(
                Constants.CredentialsErrorMessages.EmailAddressAlreadyExists,
                response.Message);
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsBadRequest_WhenPassowrdInvalid(
           SignUpRequestModel request,
           [Frozen] Mock<IUserValidationService> validationServiceMock)
        {
            // Arrange
            request.Host = "localhost";
            request.Entity!.FullName = "John Doe";
            request.Entity.EmailAddress = "john@test.com";
            request.Entity.MobilePhoneNumber = "12345678";
            request.Entity!.Password = "invalid password";

            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<String>()))
                .ReturnsAsync(true);

            var sut = CreateUserController(validationMock: validationServiceMock);

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<UserModel>(badRequest.Value);
            Assert.False(response.Success);
            Assert.Equal(
                Constants.CredentialsErrorMessages.PasswordMissing,
                response.Message);
        }

        [Theory, AutoMoqData]
        public async Task CreateUserAsync_ReturnsInternalServerError_OnException(
            SignUpRequestModel request,
            [Frozen] Mock<IUserCreationService> serviceMock,
            [Frozen] Mock<IUserValidationService> validationServiceMock,
            [Frozen] Mock<IUserSecurityService> securityMock)
        {
            // Arrange
            request.Host = "localhost";
            request.Entity!.FullName = "John Doe";
            request.Entity.EmailAddress = "john@test.com";
            request.Entity.MobilePhoneNumber = "12345678";
            request.Entity.Password = "Someone44";

            serviceMock
                .Setup(x => x.CreateUserAsync(request))
                .ThrowsAsync(new Exception(Constants.GlobalErrorMessages.UnexpectedError));

            securityMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string value, bool _) => value);

            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateUserController(creationMock: serviceMock, validationMock: validationServiceMock, securityMock: securityMock);

            // Act
            var result = await sut.CreateUserAsync(request);

            // Assert
            var statusResult = result.Should()
                .BeOfType<ObjectResult>().Subject;

            statusResult.StatusCode.Should()
                .Be(StatusCodes.Status500InternalServerError);

            var model = statusResult.Value.Should()
                .BeOfType<UserModel>().Subject;

            model.Message.Should()
                .Be(Constants.GlobalErrorMessages.UnexpectedError);
        }

        [Theory, AutoData]
        public async Task ResetPasswordAsync_ReturnsBadRequest_WhenPasswordsDoNotMatch(
            ResetPasswordRequestModel request,
            [Frozen] Mock<IUserValidationService> validationServiceMock,
            [Frozen] Mock<IUserSecurityService> securityServiceMock)
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();

            request.EmailAddress = "john@test.com";
            request.NewPassword = "Password44";
            request.NewPasswordRepeat = "Password55";

            configurationMock
               .Setup(x => x["Password:Format:LenghtRequired"])
               .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            validationServiceMock
                .Setup(x => x.DoesEmailAddressExistAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string value, bool _) => value);

            var validator =
               new ResetPasswordRequestValidator(configurationMock.Object, validationServiceMock.Object);

            var sut = CreateUserController(
                validationMock: validationServiceMock,
                securityMock: securityServiceMock);

           

            
            // Act
            var result = await sut.ResetPasswordAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<ResetPasswordResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.ResetPasswordErrorMessages.PasswordsDoNotMatch);
        }

        [Theory, AutoData]
        public async Task ChangePasswordAsync_ReturnsBadRequest_WhenNewPasswordsDoNotMatch(
            ChangePasswordRequestModel request,
            [Frozen] Mock<IUserValidationService> validationServiceMock,
            [Frozen] Mock<IUserSecurityService> securityServiceMock)
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();

            request.EmailAddress = "john@test.com";
            request.Password = "Current44";
            request.NewPassword1 = "NewPassword44";
            request.NewPassword2 = "DifferentPassword44";

            configurationMock
              .Setup(x => x["Password:Format:LenghtRequired"])
              .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");


            validationServiceMock
                .Setup(x => x.DoesEmailAddressExistAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string value, bool _) => value);

            var validator =
                new ChangePasswordRequestValidator(configurationMock.Object, validationServiceMock.Object);

            var sut = CreateUserController(
                validationMock: validationServiceMock,
                securityMock: securityServiceMock);

            // Act
            var result = await sut.ChangePasswordAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<ChangePasswordResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.ResetPasswordErrorMessages.PasswordsDoNotMatch);
        }

        [Theory, AutoData]
        public async Task ChangePasswordAsync_ReturnsBadRequest_WhenNewPasswordMatchesExistingPassword(
            ChangePasswordRequestModel request,
            [Frozen] Mock<IUserValidationService> validationServiceMock,
            [Frozen] Mock<IUserSecurityService> securityServiceMock)
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();

            request.EmailAddress = "john@test.com";
            request.Password = "Password44";
            request.NewPassword1 = "Password44";
            request.NewPassword2 = "Password44";

            configurationMock
              .Setup(x => x["Password:Format:LenghtRequired"])
              .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");


            validationServiceMock
                .Setup(x => x.DoesEmailAddressExistAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string value, bool _) => value);

            var validator =
                new ChangePasswordRequestValidator(configurationMock.Object, validationServiceMock.Object);

            var sut = CreateUserController(
                validationMock: validationServiceMock,
                securityMock: securityServiceMock);

            // Act
            var result = await sut.ChangePasswordAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<ChangePasswordResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.ChangePasswordErrorMessages.NewPasswordMatchExisting);
        }


        [Theory, AutoMoqData]
        public async Task GetUnautorizedUserByEmailAddressAsync_ReturnsUser(
            string email,
            UserModel user,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserValidationService> validationServiceMock)
        {
            // Arrange

            lookupMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            validationServiceMock
                .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateUserController(lookupMock: lookupMock);

            // Act
            var result = await sut.GetUserByEmailAddressAsync(email, true);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeSameAs(user);
        }

        [Theory, AutoMoqData]
        public async Task Update_SetsId_AndCallsService(
            UserUpdateRequestModel request,
            UserModel response,
            [Frozen] Mock<IUserCommandService> commandMock,
            [Frozen] Mock<IUserValidationService> validationServiceMock)
        {
            // Arrange
            var guid = Guid.NewGuid();
            request.FullName = "John Doe 2nd";
            request.MobilePhoneNumber = "12345678";

            commandMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserUpdateRequestModel>()))
                .ReturnsAsync(response);

            validationServiceMock
               .Setup(x => x.IsFullNameUniqueAsync(It.IsAny<string>()))
               .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateUserController(validationMock: validationServiceMock, commandMock: commandMock);

            // Act
            var result = await sut.UpdateUserAsync(guid, request);

            // Assert
            commandMock.Verify(x =>
                x.UpdateUserAsync(It.Is<UserUpdateRequestModel>(r => r.Id == guid)),
                Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory, AutoMoqData]
        public async Task VerifyEmailAddress_Publishes_WhenSuccess(
            VerificationRequestModel request,
            UserModel response,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IUserPublishingService> publishingMock)
        {
            // Arrange
            response.Success = true;

            validationMock
                .Setup(x => x.VerifyUserEmailAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            var sut = CreateUserController(validationMock: validationMock, publishingMock: publishingMock);

            // Act
            var result = await sut.VerifyEmailAddressAsync(request);

            // Assert
            publishingMock.Verify(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()), Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory, AutoMoqData]
        public async Task VerifyEmailAddress_DoesNotPublish_WhenFailed(
            VerificationRequestModel request,
            [Frozen] Mock<IUserValidationService> validationMock,
            [Frozen] Mock<IUserPublishingService> publishingMock
            )
        {
            // Arrange
            validationMock
                .Setup(x => x.VerifyUserEmailAddressAsync(It.IsAny<string>()))
                .ReturnsAsync((UserModel?)null);

            var sut = CreateUserController(validationMock: validationMock, publishingMock: publishingMock);

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
            [Frozen] Mock<IChangePasswordService> changeMock,
            [Frozen] Mock<IUserValidationService> userValidationMock,
            [Frozen] Mock<IUserSecurityService> securityMock,
            [Frozen] Mock<IUserLookupService> lookupMock,
            [Frozen] Mock<IUserPublishingService> publishingMock)
        {
            // Arrange
            request.EmailAddress = "jason@test.com";
            request.Password = "existingPassword11";
            request.NewPassword1 = "Someone33";
            request.NewPassword2 = "Someone33";

            response.Success = true;

            securityMock
               .Setup(x => x.UnprotectAsync(It.IsAny<string>(), It.IsAny<bool>()))
               .ReturnsAsync((string value, bool _) => value);

            userValidationMock
                .Setup(x => x.IsEmailAddressUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            changeMock
                .Setup(x => x.ChangePasswordAsync(request))
                .ReturnsAsync(response);

            lookupMock
                .Setup(x => x.GetSingleUserByEmailAddressAsync(request.EmailAddress!, true))
                .ReturnsAsync(user);


            var sut = CreateUserController(
                validationMock: userValidationMock,
                securityMock: securityMock,
                changeMock: changeMock,
                lookupMock: lookupMock,
                publishingMock: publishingMock);

            // Act
            var result = await sut.ChangePasswordAsync(request);

            // Assert
            publishingMock.Verify(x => x.SendUserAsync(It.IsAny<UserMessageRequestModel>()), Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }

        private static UserController CreateUserController(
            Mock<IUserCreationService>? creationMock = null,
            Mock<IUserValidationService>? validationMock = null,
            Mock<IPasswordRecoveryService>? recoveryMock = null,
            Mock<IResetPasswordService>? resetMock = null,
            Mock<IChangePasswordService>? changeMock = null,
            Mock<IUserSecurityService>? securityMock = null,
            Mock<IUserCommandService>? commandMock = null,
            Mock<IUserLookupService>? lookupMock = null,
            Mock<IUserPublishingService>? publishingMock = null,
            Mock<IConfiguration>? configurationMock = null)
        {
            creationMock ??= new();
            validationMock ??= new();
            recoveryMock ??= new();
            resetMock ??= new();
            changeMock ??= new();
            securityMock ??= new();
            commandMock ??= new();
            lookupMock ??= new();
            publishingMock ??= new();
            configurationMock ??= new();

            configurationMock
               .Setup(x => x["Password:Format:LenghtRequired"])
               .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");


            return new UserController(
                creationMock.Object,
                validationMock.Object,
                recoveryMock.Object,
                resetMock.Object,
                changeMock.Object,
                securityMock.Object,
                commandMock.Object,
                lookupMock.Object,
                publishingMock.Object,
                new SignUpRequestValidatior(configurationMock.Object, validationMock.Object),
                new UpdateUserRequestValidator(validationMock.Object),
                new ForgotPasswordRequestValidator(validationMock.Object),
                new ResetPasswordRequestValidator(configurationMock.Object, validationMock.Object),
                new ChangePasswordRequestValidator(configurationMock.Object, validationMock.Object)
            );
        }
    }

}


