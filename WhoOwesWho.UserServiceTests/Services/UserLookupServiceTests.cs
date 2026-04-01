using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using WhoOwesWho.Shared.Attributes;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using Xunit;

namespace WhoOwesWho.UserServiceTests.Services
{

    public class UserLookupServiceTests
    {
        [Theory, AutoMoqData]
        public async Task GetSingleUserByEmailAddressAsync_ReturnsUser_FromRepository(
            string email,
            UserModel expectedUser,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserLookupService sut)
        {
            queryRepository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(email, true))
                .ReturnsAsync(expectedUser);

            var result = await sut.GetSingleUserByEmailAddressAsync(email, true);

            result.Should().Be(expectedUser);
        }


        [Theory, AutoMoqData]
        public async Task GetSingleUserByEmailAddressAsync_ReturnsNull_WhenRepositoryReturnsNull(
            string email,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserLookupService sut)
        {
            queryRepository
                .Setup(x => x.GetSingleUserByEmailAddressAsync(email, false))
                .ReturnsAsync((UserModel?)null);

            var result = await sut.GetSingleUserByEmailAddressAsync(email, false);

            result.Should().BeNull();
        }


        [Theory, AutoMoqData]
        public async Task GetSingleUserByIdAsync_ReturnsUser_FromRepository(
            Guid id,
            UserModel expectedUser,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserLookupService sut)
        {
            queryRepository
                .Setup(x => x.GetSingleUserByIdAsync(id, true))
                .ReturnsAsync(expectedUser);

            var result = await sut.GetSingleUserByIdAsync(id, true);

            result.Should().Be(expectedUser);
        }


        [Theory, AutoMoqData]
        public async Task GetSingleUserByIdAsync_ReturnsNull_WhenRepositoryReturnsNull(
            Guid id,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserLookupService sut)
        {
            queryRepository
                .Setup(x => x.GetSingleUserByIdAsync(id, false))
                .ReturnsAsync((UserModel?)null);

            var result = await sut.GetSingleUserByIdAsync(id, false);

            result.Should().BeNull();
        }


        [Theory, AutoMoqData]
        public async Task GetAllUsersAsync_ReturnsUsers_FromRepository(
            List<UserModel> users,
            [Frozen] Mock<IUserQueryRepository> queryRepository,
            UserLookupService sut)
        {
            queryRepository
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            var result = await sut.GetAllUsersAsync();

            result.Should().BeEquivalentTo(users);
        }
    }
}