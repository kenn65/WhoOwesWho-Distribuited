using AutoFixture.Xunit2;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WhoOwesWho.AuthorizationService.Controllers;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Validators;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using Xunit;

namespace WhoOwesWho.AuthorizationService.Tests.Controllers
{
    public class AuthorizationControllerTests
    {
        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsOk_WhenSuccessful(
            AuthenticationRequestModel request,
            AuthenticationResponseModel response)
        {
            // Arrange
            request.Host = "localhost";
            request.EmailAddress = "kennskjellerup@hotmail.com";
            request.Password = "Password44";

            var authenticationNotificationMock =
                new Mock<IAuthenticationNotificationService>();

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            var validationServiceMock =
                new Mock<IAuthValidationService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string value, bool _) => value);

            validationServiceMock
                .Setup(x => x.DoesEmailExist(It.IsAny<string>()))
                .ReturnsAsync(true);

            validationServiceMock
                .Setup(x => x.IsPasswordValid(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            authenticationNotificationMock
                .Setup(x => x.SendAuthenticationMessageAsync(request))
                .ReturnsAsync(response);

            var sut = CreateAuthorizationController(
                authenticationNotificationMock: authenticationNotificationMock,
                securityServiceMock: securityServiceMock,
                validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var ok = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            ok.Value.Should().Be(response);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsBadRequest_WhenHostMissing(
            AuthenticationRequestModel request)
        {
            // Arrange
            request.Host = string.Empty;
            request.EmailAddress = "john@test.com";
            request.Password = "Password44";

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(request.Password))
                .ReturnsAsync(request.Password);

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Success.Should().BeFalse();

            model.Message.Should()
                .Be(Constants.GlobalErrorMessages.HostRequired);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsBadRequest_WhenEmailMissing(
            AuthenticationRequestModel request)
        {
            // Arrange
            request.Host = "localhost";
            request.EmailAddress = string.Empty;
            request.Password = "Password44";

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(request.Password))
                .ReturnsAsync(request.Password);

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.CredentialsErrorMessages.EmailAddressMissing);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsBadRequest_WhenEmailInvalid(
            AuthenticationRequestModel request)
        {
            // Arrange
            request.Host = "localhost";
            request.EmailAddress = "invalid-email";
            request.Password = "Password44";

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(request.Password))
                .ReturnsAsync(request.Password);

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.CredentialsErrorMessages.EmailAddressInvalid);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsBadRequest_WhenEmailDoesNotExist(
            AuthenticationRequestModel request)
        {
            // Arrange
            request.Host = "localhost";
            request.EmailAddress = "john@test.com";
            request.Password = "Password44";

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            var validationServiceMock =
                new Mock<IAuthValidationService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(request.Password))
                .ReturnsAsync(request.Password);

            validationServiceMock
                .Setup(x => x.DoesEmailExist(request.EmailAddress))
                .ReturnsAsync(false);

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock,
                validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.AuthenticationErrorMessages.CredentialsInvalid);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsBadRequest_WhenPasswordMissing(
            AuthenticationRequestModel request)
        {
            // Arrange
            request.Host = "localhost";
            request.EmailAddress = "john@test.com";
            request.Password = string.Empty;

            var validationServiceMock =
                new Mock<IAuthValidationService>();

            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(request.Password))
                .ReturnsAsync(request.Password);

            validationServiceMock
                .Setup(x => x.DoesEmailExist(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock, validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Message.Should()
                .Be(Constants.CredentialsErrorMessages.PasswordMissing);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_ReturnsBadRequest_WhenPasswordInvalid(
            AuthenticationRequestModel request)
        {
            // Arrange
            request.Host = "localhost";
            request.EmailAddress = "john@test.com";
            request.Password = "weak";

            var validationServiceMock =
               new Mock<IAuthValidationService>();


            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(request.Password))
                .ReturnsAsync(request.Password);

            validationServiceMock
                .Setup(x => x.DoesEmailExist(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock, validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;

            var model = badRequest.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Message.Should().Be(Constants.AuthenticationErrorMessages.CredentialsInvalid);
        }

        [Theory, AutoData]
        public async Task AuthenticateAsync_Returns500_WhenExceptionOccurs(
            AuthenticationRequestModel request)
        {
            // Arrange
            var securityServiceMock =
                new Mock<IAuthorizationSecurityService>();

            securityServiceMock
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var sut = CreateAuthorizationController(
                securityServiceMock: securityServiceMock);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var statusResult = result.Should()
                .BeOfType<ObjectResult>()
                .Subject;

            statusResult.StatusCode.Should()
                .Be(StatusCodes.Status500InternalServerError);

            var model = statusResult.Value.Should()
                .BeOfType<AuthenticationResponseModel>()
                .Subject;

            model.Message.Should().Be("Unexpected");
        }

        [Theory, AutoData]
        public async Task AuthorizeAsync_ReturnsOk_WhenSuccessful(
            AuthorizationRequestModel request,
            AuthorizationResponseModel response)
        {
            // Arrange
            request.EmailAddress = "john@test.com";
            var validationServiceMock =
                new Mock<IAuthValidationService>();

            var authorizationServiceMock =
                new Mock<IAuthorizationService>();

            authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(request))
                .ReturnsAsync(response);

            validationServiceMock
                .Setup(x => x.DoesEmailExist(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateAuthorizationController(
                authorizationServiceMock: authorizationServiceMock, validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            var ok = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            ok.Value.Should().Be(response);
        }

        [Theory, AutoData]
        public async Task AuthorizeAsync_ReturnsBadRequest_WhenExceptionOccurs(
            AuthorizationRequestModel request)
        {
            // Arrange
            request.EmailAddress = "john@test.com";

            var validationServiceMock =
                new Mock<IAuthValidationService>();

            var authorizationServiceMock =
                new Mock<IAuthorizationService>();

            authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(request))
                .ThrowsAsync(new Exception("Failure"));

            validationServiceMock
                .Setup(x => x.DoesEmailExist(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateAuthorizationController(
                authorizationServiceMock: authorizationServiceMock, validationServiceMock: validationServiceMock);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            var sc500 = result.Should()
                .BeOfType<ObjectResult>()
                .Subject;

            var model = sc500.Value.Should()
                .BeOfType<AuthorizationResponseModel>()
                .Subject;

            model.Message.Should().Be("Failure");
        }
        
        private static AuthorizationController CreateAuthorizationController(
            Mock<IAuthorizationService>? authorizationServiceMock = null,
            Mock<IAuthenticationNotificationService>? authenticationNotificationMock = null,
            Mock<IAuthorizationSecurityService>? securityServiceMock = null,
            Mock<IAuthValidationService>? validationServiceMock = null,
            Mock<IConfiguration>? configurationMock = null,
            Mock<IAuthorizationCacheRepository>? cacheRepositoryMock = null) 
         {
            authorizationServiceMock ??= new();
            authenticationNotificationMock ??= new();
            securityServiceMock ??= new();
            validationServiceMock ??= new();
            configurationMock ??= new();
            cacheRepositoryMock ??= new();

            //validationServiceMock
            //    .Setup(x => x.DoesEmailExist(It.IsAny<string>()))
            //    .ReturnsAsync(true);

            configurationMock
                .Setup(x => x["Password:Format:LenghtRequired"])
                .Returns("8");

            configurationMock
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configurationMock
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var authentivationValidator =
                new AuthenticationRequestValidatior(configurationMock.Object, validationServiceMock.Object);

            var authorizationValidator =
                new AuthorizationRequestValidator(validationServiceMock.Object);

            return new AuthorizationController(
                authorizationServiceMock.Object,
                authenticationNotificationMock.Object,
                securityServiceMock.Object,
                authentivationValidator,
                authorizationValidator,
                cacheRepositoryMock.Object);
        }
    }
}