using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WhoOwesWho.AuthorizationServiceTests.Services
{
    public class AuthorizationServiceTests
    {
        [Theory, AutoMoqData]
        public async Task Authorize_CallsUnprotectAsync(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IConfiguration> configuration,
            AuthorizationService.Services.AuthorizationService sut,
            AuthorizationRequestModel request)
        {
            // Arrange
            configuration
                .Setup(x => x["Authorization:JwtSecret"])
                .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            configuration
                .Setup(x => x["Authorization:Issuer"])
                .Returns("WhoOwesWho App");

            configuration
                .Setup(x => x["Authorization:Audience"])
                .Returns("WhoOwesWho Audience");

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(It.IsAny<string>()))
                .ReturnsAsync("decrypted@test.com");

            // Act
            await sut.Authorize(request);

            // Assert
            authorizationSecurityService.Verify(x => x.UnprotectAsync(It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Authorize_CallsRepository_WithDecryptedEmail(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthorizationService.Services.AuthorizationService sut,
            AuthorizationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            configuration
               .Setup(x => x["Authorization:JwtSecret"])
               .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            configuration
                .Setup(x => x["Authorization:Issuer"])
                .Returns("WhoOwesWho App");

            configuration
                .Setup(x => x["Authorization:Audience"])
                .Returns("WhoOwesWho Audience");

            var decryptedEmail = "decrypted@test.com";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress!))
                .ReturnsAsync(decryptedEmail);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(decryptedEmail))
                .ReturnsAsync(user);

            // Act
            await sut.Authorize(request);

            // Assert
            authorizationCacheRepository.Verify(x => x.GetUserAsync(decryptedEmail), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Authorize_CallsProtectCookiesAsync(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthorizationService.Services.AuthorizationService sut,
            AuthorizationRequestModel request,
            UserMessageResponseModel user,
            AuthorizationResponseModel expected)
        {
            // Arrange
            configuration
              .Setup(x => x["Authorization:JwtSecret"])
              .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            configuration
                .Setup(x => x["Authorization:Issuer"])
                .Returns("WhoOwesWho App");

            configuration
                .Setup(x => x["Authorization:Audience"])
                .Returns("WhoOwesWho Audience");

            var decryptedEmail = "test@test.com";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress!))
                .ReturnsAsync(decryptedEmail);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(decryptedEmail))
                .ReturnsAsync(user);

            authorizationSecurityService
                .Setup(x => x.ProtectCookiesAsync(user, It.IsAny<string>(), true))
                .ReturnsAsync(expected);

            // Act
            var result = await sut.Authorize(request);

            // Assert
            result.Should().Be(expected);

            authorizationSecurityService.Verify(
                x => x.ProtectCookiesAsync(user, It.IsAny<string>(), true),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Authorize_GeneratesToken_WithCorrectClaims(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthorizationService.Services.AuthorizationService sut,
            AuthorizationRequestModel request,
            UserMessageResponseModel user)
        {
            // Arrange
            configuration
              .Setup(x => x["Authorization:JwtSecret"])
              .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            configuration
                .Setup(x => x["Authorization:Issuer"])
                .Returns("WhoOwesWho App");

            configuration
                .Setup(x => x["Authorization:Audience"])
                .Returns("WhoOwesWho Audience");

            var decryptedEmail = "test@test.com";

            user.Id = Guid.NewGuid();
            user.EmailAddress = decryptedEmail;
            user.FullName = "Test User";
            user.Admin = true;

            string? capturedToken = null;

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress!))
                .ReturnsAsync(decryptedEmail);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(decryptedEmail))
                .ReturnsAsync(user);

            authorizationSecurityService
                .Setup(x => x.ProtectCookiesAsync(user, It.IsAny<string>(), true))
                .Callback<UserMessageResponseModel, string, bool>((_, token, _) => capturedToken = token)
                .ReturnsAsync(new AuthorizationResponseModel());

            // Act
            await sut.Authorize(request);

            // Assert
            capturedToken.Should().NotBeNull();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(capturedToken);

            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.EmailAddress);
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == user.FullName);
            jwt.Claims.Should().Contain(c => c.Type == "admin" && c.Value == user.Admin.ToString());
        }

        [Theory, AutoMoqData]
        public async Task Authorize_ReturnsResponse_FromProtectCookies(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            [Frozen] Mock<IConfiguration> configuration,
            AuthorizationService.Services.AuthorizationService sut,
            AuthorizationRequestModel request,
            UserMessageResponseModel user,
            AuthorizationResponseModel expected)
        {
            // Arrange
            configuration
             .Setup(x => x["Authorization:JwtSecret"])
             .Returns("i4Ifq0YmvlsydD2IDFgkLC8IOjiTGQoGTNjJH2KaR30LUjOCs0nxTq4iTdzTmCM3uDYnisM4c5AfACDbABtzVA==");

            configuration
                .Setup(x => x["Authorization:Issuer"])
                .Returns("WhoOwesWho App");

            configuration
                .Setup(x => x["Authorization:Audience"])
                .Returns("WhoOwesWho Audience");

            var email = "test@test.com";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress!))
                .ReturnsAsync(email);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);

            authorizationSecurityService
                .Setup(x => x.ProtectCookiesAsync(user, It.IsAny<string>(), true))
                .ReturnsAsync(expected);

            // Act
            var result = await sut.Authorize(request);

            // Assert
            result.Should().Be(expected);
        }

        [Theory, AutoMoqData]
        public async Task Authorize_Throws_WhenUserIsNull(
            [Frozen] Mock<IAuthorizationSecurityService> authorizationSecurityService,
            [Frozen] Mock<IAuthorizationCacheRepository> authorizationCacheRepository,
            AuthorizationService.Services.AuthorizationService sut,
            AuthorizationRequestModel request)
        {
            // Arrange
            var email = "test@test.com";

            authorizationSecurityService
                .Setup(x => x.UnprotectAsync(request.EmailAddress!))
                .ReturnsAsync(email);

            authorizationCacheRepository
                .Setup(x => x.GetUserAsync(email))
                .ReturnsAsync((UserMessageResponseModel?)null);

            // Act
            Func<Task> act = async () => await sut.Authorize(request);

            // Assert
            await act.Should().ThrowAsync<NullReferenceException>();
        }
    }
}
