namespace Raffle.Api.Exceptions
{
    public class RaffleNotFoundException : DomainException
    {
        public RaffleNotFoundException(string raffleName) : base($"Raffle {raffleName} was not found.")
        {
        }
    }
}
