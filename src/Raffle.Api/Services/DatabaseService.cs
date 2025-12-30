using Microsoft.EntityFrameworkCore;
using Raffle.Api.Contracts;
using Raffle.Api.Database;
using Raffle.Api.Models;

namespace Raffle.Api.Services
{
    public interface IDatabaseService
    {
        Task<bool> RaffleMemberExists(Guid raffleDrawId, string email);
        Task<List<WinnerDto>> ClosedRaffleDraws();
        Task CloseRaffleDraw(RaffleDraw raffleDraw, Guid winnerId);
        Task AddRaffleDraw(RaffleDraw raffle);
        Task<RaffleDraw?> RaffleDrawByName(string name);
        Task AddRaffleMember(RaffleMember member);
        Task AddMembersToRaffle(List<RaffleMember> raffleMembers);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly RaffleDbContext _database;

        public DatabaseService(RaffleDbContext database)
        {
            _database = database;
        }

        public async Task AddRaffleDraw(RaffleDraw raffle)
        {
            _database.RaffleDraws.Add(raffle);
            await _database.SaveChangesAsync();
        }

        public async Task<RaffleDraw?> RaffleDrawByName(string raffleDrawName)
        {
            return await _database.RaffleDraws
                .Include(r => r.RaffleMembers)
                .FirstOrDefaultAsync(r => r.Name == raffleDrawName);
        }

        public async Task CloseRaffleDraw(RaffleDraw raffleDraw, Guid winnerId)
        {
            raffleDraw.IsClosed = true;
            raffleDraw.ClosedAt = DateTime.UtcNow;
            raffleDraw.WinnerMemberId = winnerId;

            await _database.SaveChangesAsync();
        }

        public async Task<List<WinnerDto>> ClosedRaffleDraws()
        {
            return await _database.RaffleDraws
                .Where(r => r.IsClosed && r.WinnerMemberId != null)
                .Select(r => new WinnerDto(
                    r.WinnerMember.Id,
                    r.Id,
                    r.WinnerMember.Name,
                    r.WinnerMember.Email,
                    r.WinnerMember.CreatedAt
                    ))
                .ToListAsync();
        }

        public async Task AddRaffleMember(RaffleMember member)
        {
            _database.Members.Add(member);
            await _database.SaveChangesAsync();
        }

        public async Task AddMembersToRaffle(List<RaffleMember> raffleMembers)
        {
            await _database.Members.AddRangeAsync(raffleMembers);
            await _database.SaveChangesAsync();
        }

        public async Task<bool> RaffleMemberExists(Guid raffleDrawId, string email)
        {
            return await _database.Members
                .AnyAsync(m => m.RaffleDrawId == raffleDrawId && m.Email == email);
        }
    }
}
