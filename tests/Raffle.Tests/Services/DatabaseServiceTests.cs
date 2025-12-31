using FluentAssertions;
using Raffle.Api.Models;
using Raffle.Api.Services;
using Raffle.Tests.Common;


namespace Raffle.Tests.Services
{
    public class DatabaseServiceTests
    {
        [Fact]
        public async Task AddRaffleDraw_ShouldPersistRaffle()
        {
            var context = DbContextFactory.Create();
            var service = new DatabaseService(context);

            var raffleDrawName = "TestRaffle";
            var raffleDraw = new RaffleDraw
            {
                Name = raffleDrawName
            };

            await service.AddRaffleDraw(raffleDraw);

            context.RaffleDraws.Should().ContainSingle(r => r.Name == raffleDrawName);
        }

        [Fact]
        public async Task RaffleMemberExistsAsync_ReturnsTrue_WhenMemberExists()
        {
            var context = DbContextFactory.Create();
            var raffleId = Guid.NewGuid();

            var raffleMember = new RaffleMember
            {
                Name = "John",
                Email = "john@mail.com",
                RaffleDrawId = raffleId
            };

            context.Members.Add(raffleMember);

            await context.SaveChangesAsync();

            var service = new DatabaseService(context);

            var exists = await service.RaffleMemberExists(raffleId, "john@mail.com");

            exists.Should().BeTrue();
        }
    }
}
