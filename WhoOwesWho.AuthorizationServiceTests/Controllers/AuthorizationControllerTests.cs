using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WhoOwesWho.AuthorizationService.Controllers;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using Xunit;

namespace WhoOwesWho.AuthorizationServiceTests.Controllers
{
    public class AuthorizationControllerTests
    {
        [Theory, AutoMoqData]
        public async Task Authenticate_ReturnsOk_WithResponse(
             [Frozen] Mock<IAuthenticationNotificationService> authenticationNotificationService,
             [Frozen] Mock<IAuthorizationService> authorizationService,
             AuthenticationRequestModel request,
             AuthenticationResponseModel response)
                {
            // Arrange
            authenticationNotificationService
                .Setup(x => x.SendAuthenticationMessageAsync(request))
                .ReturnsAsync(response);

            var sut = new AuthorizationController(
                authorizationService.Object,
                authenticationNotificationService.Object);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().Be(response);
        }

        [Theory, AutoMoqData]
        public async Task Authenticate_CallsService_Once(
            [Frozen] Mock<IAuthenticationNotificationService> authenticationNotificationService,
            [Frozen] Mock<IAuthorizationService> authorizationService,
            AuthenticationRequestModel request,
            AuthenticationResponseModel response)
        {
            // Arrange
            authenticationNotificationService
                .Setup(x => x.SendAuthenticationMessageAsync(request))
                .ReturnsAsync(response);

            var sut = new AuthorizationController(
                authorizationService.Object,
                authenticationNotificationService.Object);

            // Act
            await sut.AuthenticateAsync(request);

            // Assert
            authenticationNotificationService.Verify(
                x => x.SendAuthenticationMessageAsync(request),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Authenticate_ReturnsBadRequest_WhenExceptionThrown(
            [Frozen] Mock<IAuthenticationNotificationService> authenticationNotificationService,
            [Frozen] Mock<IAuthorizationService> authorizationService,
            AuthenticationRequestModel request)
        {
            // Arrange
            authenticationNotificationService
                .Setup(x => x.SendAuthenticationMessageAsync(request))
                .ThrowsAsync(new Exception("error"));

            var sut = new AuthorizationController(
                authorizationService.Object,
                authenticationNotificationService.Object);

            // Act
            var result = await sut.AuthenticateAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("error");
        }

        [Theory, AutoMoqData]
        public async Task Authorize_ReturnsOk_WithResponse(
            [Frozen] Mock<IAuthenticationNotificationService> authenticationNotificationService,
            [Frozen] Mock<IAuthorizationService> authorizationService,
            AuthorizationRequestModel request,
            AuthorizationResponseModel response)
        {
            // Arrange
            authorizationService
                .Setup(x => x.AuthorizeAsync(request))
                .ReturnsAsync(response);

            var sut = new AuthorizationController(
               authorizationService.Object,
               authenticationNotificationService.Object);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().Be(response);
        }

        [Theory, AutoMoqData]
        public async Task Authorize_CallsService_Once(
            [Frozen] Mock<IAuthenticationNotificationService> authenticationNotificationService,
            [Frozen] Mock<IAuthorizationService> authorizationService,
            AuthorizationRequestModel request,
            AuthorizationResponseModel response)
        {
            // Arrange
            authorizationService
                .Setup(x => x.AuthorizeAsync(request))
                .ReturnsAsync(response);

            var sut = new AuthorizationController(
              authorizationService.Object,
              authenticationNotificationService.Object);

            // Act
            await sut.AuthorizeAsync(request);

            // Assert
            authorizationService.Verify(
                x => x.AuthorizeAsync(request),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Authorize_ReturnsBadRequest_WhenExceptionThrown(
            [Frozen] Mock<IAuthenticationNotificationService> authenticationNotificationService,
            [Frozen] Mock<IAuthorizationService> authorizationService,
            AuthorizationRequestModel request)
        {
            // Arrange
            authorizationService
                .Setup(x => x.AuthorizeAsync(request))
                .ThrowsAsync(new Exception("error"));

            var sut = new AuthorizationController(
             authorizationService.Object,
             authenticationNotificationService.Object);

            // Act
            var result = await sut.AuthorizeAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("error");
        }
    }
}
