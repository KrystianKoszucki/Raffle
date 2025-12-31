using FluentAssertions;
using Moq;
using Raffle.Api.Contracts;
using Raffle.Api.Exceptions;
using Raffle.Api.Models;
using Raffle.Api.Services;

namespace Raffle.Tests.Services;

public class RaffleServiceTests
{
    private readonly Mock<IDatabaseService> _databaseMock;
    private readonly RaffleService _raffleService;

    public RaffleServiceTests()
    {
        _databaseMock = new Mock<IDatabaseService>();

        _raffleService = new RaffleService(_databaseMock.Object);
    }

    [Fact]
    public async Task CreateRaffleDraw_ShouldCreate_WhenNotExists()
    {
        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync((RaffleDraw?)null);

        await _raffleService.CreateRaffle("Test");

        _databaseMock.Verify(
            d => d.AddRaffleDraw(It.Is<RaffleDraw>(r => r.Name == "Test")),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateRaffleDrawAsync_ShouldThrow_WhenAlreadyExists()
    {
        var raffleDraw = new RaffleDraw { Name = "Test" };
        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync(raffleDraw);

        var act = () => _raffleService.CreateRaffle("Test");

        await act.Should().ThrowAsync<RaffleDrawAlreadyExistsException>();
    }

    [Fact]
    public async Task EnterRaffle_ShouldAddMember_WhenValid()
    {
        var raffleDraw = new RaffleDraw
        {
            Name = "TestRaffle",
            IsClosed = false
        };

        _databaseMock.Setup(d => d.RaffleDrawByName(raffleDraw.Name))
            .ReturnsAsync(raffleDraw);
        _databaseMock.Setup(d => d.RaffleMemberExists(raffleDraw.Id, "member@test.com"))
            .ReturnsAsync(false);

        await _raffleService.EnterRaffle(raffleDraw.Name, "Member Name", "member@test.com");

        _databaseMock.Verify(d => d.AddRaffleMember(It.Is<RaffleMember>(
            m => m.Name == "Member Name" &&
                 m.Email == "member@test.com" &&
                 m.RaffleDrawId == raffleDraw.Id
        )), Times.Once);
    }

    [Fact]
    public async Task EnterRaffle_ShouldThrowRaffleNotFound_WhenRaffleDoesNotExist()
    {
        _databaseMock.Setup(d => d.RaffleDrawByName("NonExistentRaffle"))
            .ReturnsAsync((RaffleDraw)null);

        await _raffleService
            .Invoking(s => s.EnterRaffle("NonExistentRaffle", "Name", "email@test.com"))
            .Should().ThrowAsync<RaffleNotFoundException>();
    }

    [Fact]
    public async Task EnterRaffle_ShouldThrowRaffleClosed_WhenRaffleIsClosed()
    {
        var raffleDraw = new RaffleDraw { Id = Guid.NewGuid(), IsClosed = true };
        _databaseMock.Setup(d => d.RaffleDrawByName("ClosedRaffle"))
            .ReturnsAsync(raffleDraw);

        await _raffleService
            .Invoking(s => s.EnterRaffle("ClosedRaffle", "Name", "email@test.com"))
            .Should().ThrowAsync<RaffleClosedException>();
    }

    [Fact]
    public async Task EnterRaffle_ShouldThrowDuplicateRaffleMember_WhenEmailAlreadyExists()
    {
        var raffleDraw = new RaffleDraw { Id = Guid.NewGuid(), IsClosed = false };
        _databaseMock.Setup(d => d.RaffleDrawByName("TestRaffle"))
            .ReturnsAsync(raffleDraw);
        _databaseMock.Setup(d => d.RaffleMemberExists(raffleDraw.Id, "duplicate@test.com"))
            .ReturnsAsync(true);

        await _raffleService
            .Invoking(s => s.EnterRaffle("TestRaffle", "Name", "duplicate@test.com"))
            .Should().ThrowAsync<DuplicateRaffleMemberException>();
    }

    [Fact]
    public async Task AddRaffleMembers_ShouldAddMembers_WhenValid()
    {
        var raffleDraw = new RaffleDraw { Name = "Test" };

        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync(raffleDraw);

        _databaseMock
            .Setup(d => d.RaffleMemberExists(raffleDraw.Id, It.IsAny<string>()))
            .ReturnsAsync(false);

        var members = new List<EnterRaffleDrawRequest>
        {
            new("Anna", "anna@test.com"),
            new("Bob", "bob@test.com")
        };

        await _raffleService.AddRaffleMembers("Test", members);

        _databaseMock.Verify(d =>
            d.AddMembersToRaffle(
                It.Is<List<RaffleMember>>(list =>
                    list.Count == 2 &&
                    list.Any(m => m.Email == "anna@test.com") &&
                    list.Any(m => m.Email == "bob@test.com")
                )),
            Times.Once);
    }

    [Fact]
    public async Task AddRaffleMembers_ShouldThrow_WhenRaffleNotFound()
    {
        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync((RaffleDraw?)null);
        var members = new List<EnterRaffleDrawRequest>
        {
            new("Anna", "anna@test.com")
        };

        var act = () => _raffleService.AddRaffleMembers("Test", members);

        await act.Should().ThrowAsync<RaffleNotFoundException>();
    }

    [Fact]
    public async Task AddRaffleMembers_ShouldThrow_WhenRaffleClosed()
    {
        var closedRaffleDraw = new RaffleDraw { Name = "Test", IsClosed = true };
        var members = new List<EnterRaffleDrawRequest>
        {
            new("Anna", "anna@test.com")
        };

        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync(closedRaffleDraw);

        var act = () => _raffleService.AddRaffleMembers("Test", members);

        await act.Should().ThrowAsync<RaffleClosedException>();
    }

    [Fact]
    public async Task AddRaffleMembers_ShouldThrow_OnDuplicateEmailInRequest()
    {
        var raffleDraw = new RaffleDraw { Name = "Test" };

        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync(raffleDraw);

        var members = new List<EnterRaffleDrawRequest>
        {
            new("Anna", "duplicate@test.com"),
            new("Bob", "duplicate@test.com")
        };

        var act = () => _raffleService.AddRaffleMembers("Test", members);

        await act.Should().ThrowAsync<DuplicateRaffleMemberException>();
    }

    [Fact]
    public async Task AddRaffleMembers_ShouldThrow_OnDuplicateEmailInDatabase()
    {
        var raffleDraw = new RaffleDraw { Name = "Test" };
        var members = new List<EnterRaffleDrawRequest>
        {
            new("Anna", "duplicate@test.com")
        };

        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync(raffleDraw);

        _databaseMock
            .Setup(d => d.RaffleMemberExists(raffleDraw.Id, "duplicate@test.com"))
            .ReturnsAsync(true);

        var act = () => _raffleService.AddRaffleMembers("Test", members);

        await act.Should().ThrowAsync<DuplicateRaffleMemberException>();
    }

    [Fact]
    public async Task CloseRaffleDrawAndPickWinnerAsync_ShouldThrowNoRaffleMembersException_WhenNoMembers()
    {
        var raffleDraw = new RaffleDraw { Name = "Test" };

        _databaseMock
            .Setup(d => d.RaffleDrawByName("Test"))
            .ReturnsAsync(raffleDraw);

        var members = new List<EnterRaffleDrawRequest>
        {
            new("Anna", "anna@test.com"),
            new("Bob", "bob@test.com")
        };

        _databaseMock.Setup(d => d.RaffleDrawByName("TestRaffle"))
                     .ReturnsAsync(raffleDraw);

        await _raffleService.AddRaffleMembers("Test", members);

        Func<Task> act = async () => await _raffleService.CloseRaffleDrawAndPickWinner(raffleDraw.Name);

        await act.Should().ThrowAsync<NoRaffleMembersException>();

        raffleDraw.IsClosed.Should().BeFalse();
        raffleDraw.WinnerMember.Should().BeNull();

    }

    [Fact]
    public async Task CloseRaffleDrawAndPickWinner_ShouldCloseRaffleAndReturnWinner_WhenMembersExist()
    {
        var raffleDraw = new RaffleDraw
        {
            Name = "TestRaffle",
            RaffleMembers = new List<RaffleMember>
                {
                    new () { Name = "Anna", Email = "anna@test.com" },
                    new () { Name = "Bob", Email = "bob@test.com" }
                }
        };

        _databaseMock.Setup(d => d.RaffleDrawByName("TestRaffle"))
            .ReturnsAsync(raffleDraw);

        _databaseMock
            .Setup(d => d.CloseRaffleDraw(raffleDraw, It.IsAny<Guid>()))
            .Callback<RaffleDraw, Guid>((rd, winnerId) =>
            {
                rd.IsClosed = true;
                rd.WinnerMember = rd.RaffleMembers.First(m => m.Id == winnerId);
            })
            .Returns(Task.CompletedTask);

        var winner = await _raffleService.CloseRaffleDrawAndPickWinner("TestRaffle");

        raffleDraw.IsClosed.Should().BeTrue();
        raffleDraw.WinnerMember.Should().NotBeNull();
        raffleDraw.RaffleMembers.Should().Contain(raffleDraw.WinnerMember);
        winner.Id.Should().Be(raffleDraw.WinnerMember.Id);

        _databaseMock.Verify(d => d.CloseRaffleDraw(raffleDraw, winner.Id), Times.Once);
    }

    [Fact]
    public async Task CloseRaffleDrawAndPickWinner_ShouldThrow_WhenRaffleDoesNotExist()
    {
        _databaseMock.Setup(d => d.RaffleDrawByName("NonExistent"))
            .ReturnsAsync((RaffleDraw)null);

        await _raffleService
            .Invoking(s => s.CloseRaffleDrawAndPickWinner("NonExistent"))
            .Should().ThrowAsync<RaffleNotFoundException>();
    }

    [Fact]
    public async Task CloseRaffleDrawAndPickWinner_ShouldThrow_WhenRaffleIsAlreadyClosed()
    {
        var raffleDraw = new RaffleDraw
        {
            Name = "ClosedRaffle",
            IsClosed = true
        };

        _databaseMock.Setup(d => d.RaffleDrawByName("ClosedRaffle"))
            .ReturnsAsync(raffleDraw);

        await _raffleService
            .Invoking(s => s.CloseRaffleDrawAndPickWinner("ClosedRaffle"))
            .Should().ThrowAsync<RaffleClosedException>();
    }

    [Fact]
    public async Task CloseRaffleDrawAndPickWinner_ShouldThrow_WhenNoMembers()
    {
        var raffleDraw = new RaffleDraw
        {
            Name = "EmptyRaffle",
            RaffleMembers = new List<RaffleMember>()
        };

        _databaseMock.Setup(d => d.RaffleDrawByName("EmptyRaffle"))
            .ReturnsAsync(raffleDraw);

        await _raffleService
            .Invoking(s => s.CloseRaffleDrawAndPickWinner("EmptyRaffle"))
            .Should().ThrowAsync<NoRaffleMembersException>();
    }

    [Fact]
    public async Task GetPastWinners_ShouldReturnList_WhenClosedRafflesExist()
    {
        var winner1 = new RaffleMember
        {
            Name = "Anna",
            Email = "anna@test.com",
        };
        var winner2 = new RaffleMember
        {
            Name = "Bob",
            Email = "bob@test.com",
        };

        var closedRaffles = new List<WinnerDto>
            {
                new(winner1.Id, Guid.NewGuid(), winner1.Name, winner1.Email, winner1.CreatedAt),
                new(winner2.Id, Guid.NewGuid(), winner2.Name, winner2.Email, winner2.CreatedAt)
            };

        _databaseMock.Setup(d => d.ClosedRaffleDraws())
            .ReturnsAsync(closedRaffles);

        var result = await _raffleService.GetPastWinners();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(closedRaffles);
        _databaseMock.Verify(d => d.ClosedRaffleDraws(), Times.Once);
    }

    [Fact]
    public async Task GetPastWinners_ShouldThrow_WhenNoClosedRaffles()
    {
        _databaseMock.Setup(d => d.ClosedRaffleDraws())
            .ReturnsAsync(new List<WinnerDto>());

        await _raffleService
            .Invoking(s => s.GetPastWinners())
            .Should().ThrowAsync<NoClosedRaffleDrawsException>();

        _databaseMock.Verify(d => d.ClosedRaffleDraws(), Times.Once);
    }

    [Fact]
    public async Task GetPastWinners_ShouldThrow_WhenNullReturnedFromDatabase()
    {
        _databaseMock.Setup(d => d.ClosedRaffleDraws())
            .ReturnsAsync((List<WinnerDto>)null);

        await _raffleService
            .Invoking(s => s.GetPastWinners())
            .Should().ThrowAsync<NoClosedRaffleDrawsException>();

        _databaseMock.Verify(d => d.ClosedRaffleDraws(), Times.Once);
    }
}