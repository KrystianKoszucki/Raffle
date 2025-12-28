using Raffle.Api.Contracts;
using Raffle.Api.Models;
using System.Security.Cryptography;

namespace Raffle.Api.Services
{
    public interface IRaffleService
    {
        Task<RaffleMember> CloseRaffleDrawAndPickWinner(string raffleDrawName);
        Task CreateRaffle(string name);
        Task EnterRaffle(string raffleDrawName, string memberName, string memberEmail);
        Task<List<WinnerDto>> GetPastWinners();
        Task AddRaffleMembers(string raffleDrawName, List<EnterRaffleDrawRequest> raffleMembersDto);

    }

    public class RaffleService : IRaffleService
    {
        private readonly IDatabaseService _database;

        public RaffleService(IDatabaseService database)
        {
            _database = database;
        }

        public async Task CreateRaffle(string name)
        {
            var raffleDraw = await _database.RaffleDrawByName(name) ;
            if (raffleDraw != null)
                throw new InvalidOperationException("RaffleDraw already exists");

            raffleDraw = new RaffleDraw
            {
                Name = name,
                IsClosed = false
            };
            await _database.AddRaffleDraw(raffleDraw);
        }

        public async Task EnterRaffle(string raffleDrawName, string memberName, string memberEmail)
        {
            var raffleDraw = await _database.RaffleDrawByName(raffleDrawName);
            if (raffleDraw == null)
                throw new KeyNotFoundException("RaffleDraw not found");

            if (raffleDraw.IsClosed)
                throw new InvalidOperationException("RaffleDraw is closed");

            var member = new RaffleMember
            {
                Name = memberName,
                Email = memberEmail,
                RaffleDrawId = raffleDraw.Id
            };

            await _database.AddRaffleMember(member);
        }

        public async Task<RaffleMember> CloseRaffleDrawAndPickWinner(string raffleDrawName)
        {
            var raffleDraw = await _database.RaffleDrawByName(raffleDrawName);
            if (raffleDraw == null)
                throw new KeyNotFoundException("RaffleDraw not found");

            if (raffleDraw.IsClosed)
                throw new InvalidOperationException("RaffleDraw is closed");

            if (!raffleDraw.RaffleMembers.Any())
                throw new InvalidOperationException("RaffleDraw does not have any members");

            var winner = PickWinner(raffleDraw.RaffleMembers.ToList());
            await _database.CloseRaffleDraw(raffleDraw, winner.Id);

            return winner;
        }

        public async Task<List<WinnerDto>> GetPastWinners()
        {
            var closedRaffleDraws = await _database.ClosedRaffleDraws();
            if (closedRaffleDraws == null || closedRaffleDraws.Count() == 0)
                throw new InvalidOperationException("No RaffleDraw has been closed");

            return closedRaffleDraws;
        }

        public async Task AddRaffleMembers(string raffleDrawName, List<EnterRaffleDrawRequest> raffleMembersDto)
        {
            var raffleDraw = await _database.RaffleDrawByName(raffleDrawName);
            if (raffleDraw == null)
                throw new KeyNotFoundException("RaffleDraw not found");

            if (raffleDraw.IsClosed)
                throw new InvalidOperationException("RaffleDraw is closed");

            var raffleMembers = raffleMembersDto.Select(r => new RaffleMember
            {
                Id = Guid.NewGuid(),
                Name = r.Name,
                Email = r.Email,
                RaffleDrawId = raffleDraw.Id
            }).ToList();

            await _database.AddMembersToRaffle(raffleMembers);
        }

        private RaffleMember PickWinner(List<RaffleMember> members)
        {
            if (members == null || members.Count == 0)
                throw new KeyNotFoundException("Non members were found");

            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            int winnerIndex = value % members.Count;

            return members[winnerIndex];
        }
    }
}
