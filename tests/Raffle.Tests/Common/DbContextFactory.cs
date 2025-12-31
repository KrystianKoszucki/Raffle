using Microsoft.EntityFrameworkCore;
using Raffle.Api.Database;

namespace Raffle.Tests.Common
{
    public static class DbContextFactory
    {
        public static RaffleDbContext Create()
        {
            var options = new DbContextOptionsBuilder<RaffleDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaffleDbContext(options);
        }
    }
}
