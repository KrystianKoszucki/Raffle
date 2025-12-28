namespace Raffle.Api.Contracts
{
    public record WinnerDto(Guid Id, Guid RaffleDrawId, string Name, string Email, DateTime CreatedAt);
}
