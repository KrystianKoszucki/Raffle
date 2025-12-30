using Raffle.Api.Contracts;
using Raffle.Api.Exceptions;
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

        public async Task CreateRaffle(string raffleDrawName)
        {
            var raffleDraw = await _database.RaffleDrawByName(raffleDrawName) ;
            if (raffleDraw != null)
                throw new RaffleDrawAlreadyExistsException(raffleDrawName);

            raffleDraw = new RaffleDraw
            {
                Name = raffleDrawName,
                IsClosed = false
            };
            await _database.AddRaffleDraw(raffleDraw);
        }

        public async Task EnterRaffle(string raffleDrawName, string memberName, string memberEmail)
        {
            var raffleDraw = await _database.RaffleDrawByName(raffleDrawName);
            if (raffleDraw == null)
                throw new RaffleNotFoundException(raffleDrawName);

            if (raffleDraw.IsClosed)
                throw new RaffleClosedException(raffleDrawName);

            var emailExistsInRaffleDraw = await _database.RaffleMemberExists(raffleDraw.Id, memberEmail);
            if (emailExistsInRaffleDraw)
                throw new DuplicateRaffleMemberException(memberEmail);

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
                throw new RaffleNotFoundException(raffleDrawName);

            if (raffleDraw.IsClosed)
                throw new RaffleClosedException(raffleDrawName);

            if (!raffleDraw.RaffleMembers.Any())
                throw new NoRaffleMembersException(raffleDrawName);

            var winner = PickWinner(raffleDraw.RaffleMembers.ToList());
            await _database.CloseRaffleDraw(raffleDraw, winner.Id);

            return winner;
        }

        public async Task<List<WinnerDto>> GetPastWinners()
        {
            var closedRaffleDraws = await _database.ClosedRaffleDraws();
            if (closedRaffleDraws == null || closedRaffleDraws.Count() == 0)
                throw new NoClosedRaffleDrawsException();

            return closedRaffleDraws;
        }

        public async Task AddRaffleMembers(string raffleDrawName, List<EnterRaffleDrawRequest> raffleMembersDto)
        {
            var raffleDraw = await _database.RaffleDrawByName(raffleDrawName);
            if (raffleDraw == null)
                throw new RaffleNotFoundException(raffleDrawName);

            if (raffleDraw.IsClosed)
                throw new RaffleClosedException(raffleDrawName);

            var checkedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var member in raffleMembersDto)
            {
                if (!checkedEmails.Add(member.Email))
                    throw new DuplicateRaffleMemberException(member.Email);

                var existsInRaffleDraw = await _database.RaffleMemberExists(raffleDraw.Id, member.Email);
                if (existsInRaffleDraw)
                    throw new DuplicateRaffleMemberException(member.Email);
            }

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
                throw new NoMembersProvidedException();

            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            int winnerIndex = value % members.Count;

            return members[winnerIndex];
        }
    }
}
