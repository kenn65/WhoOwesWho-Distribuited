using AutoFixture.Xunit2;
using Castle.Core.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WhoOwesWho.AuthorizationServiceTests.Services
{
    public class AuthenticationValidationServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_Throws_WhenEmailMissing(
            AuthenticationValidationService sut)
        {
            // Act
            Func<Task> act = async () => await sut.ValidateUserCredentialsAsync(null!, "password");

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Email and/or password was not provided");
        }

        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_Throws_WhenPasswordMissing(
            AuthenticationValidationService sut)
        {
            // Act
            Func<Task> act = async () => await sut.ValidateUserCredentialsAsync("test@test.com", null!);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Email and/or password was not provided");
        }


        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_ReturnsFalse_WhenEmailInvalid(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            AuthenticationValidationService sut)
        {
            // Arrange
            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync("invalid-email");

            // Act
            var result = await sut.ValidateUserCredentialsAsync("encrypted", "Password1");

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_ReturnsFalse_WhenPasswordInvalid(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IConfiguration> configuration,
            AuthenticationValidationService sut)
        {
            // Arrange
            configuration
               .Setup(x => x["Password:Format:LenghtRequired"])
               .Returns("8");

            configuration
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configuration
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync("valid@test.com");

            var invalidPassword = "short";

            // Act
            var result = await sut.ValidateUserCredentialsAsync("encrypted", invalidPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_ReturnsFalse_WhenUserNotFound(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthenticationValidationService sut)
        {
            // Arrange
            configuration
             .Setup(x => x["Password:Format:LenghtRequired"])
             .Returns("8");

            configuration
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configuration
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var email = "valid@test.com";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync(email);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync((UserMessageResponseModel?)null);

            // Act
            var result = await sut.ValidateUserCredentialsAsync("encrypted", "Password1");

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_ReturnsFalse_WhenEmailNotVerified(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthenticationValidationService sut,
            UserMessageResponseModel user)
        {
            // Arrange
            configuration
               .Setup(x => x["Password:Format:LenghtRequired"])
               .Returns("8");

            configuration
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configuration
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var email = "valid@test.com";
            user.EmailAddressVerified = false;
            user.Password = "Password1";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync(email);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await sut.ValidateUserCredentialsAsync("encrypted", "Password1");

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_ReturnsFalse_WhenPasswordDoesNotMatch(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthenticationValidationService sut,
            UserMessageResponseModel user)
        {
            // Arrange
            configuration
             .Setup(x => x["Password:Format:LenghtRequired"])
             .Returns("8");

            configuration
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configuration
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var email = "valid@test.com";
            user.EmailAddressVerified = true;
            user.Password = "CorrectPassword1";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync(email);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await sut.ValidateUserCredentialsAsync("encrypted", "WrongPassword1");

            // Assert
            result.Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task ValidateUserCredentialsAsync_ReturnsTrue_WhenValid(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthenticationValidationService sut,
            UserMessageResponseModel user)
        {
            // Arrange
            configuration
             .Setup(x => x["Password:Format:LenghtRequired"])
             .Returns("8");

            configuration
                .Setup(x => x["Password:Format:UppercaseRequired"])
                .Returns("1");

            configuration
                .Setup(x => x["Password:Format:DigitsRequired"])
                .Returns("2");

            var email = "valid@test.com";
            var password = "Password123";

            user.EmailAddressVerified = true;
            user.Password = password;

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync(email);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await sut.ValidateUserCredentialsAsync("encrypted", password);

            // Assert
            result.Should().BeTrue();
        }
    }
}
