namespace Raffle.Api.Exceptions
{
    public class RaffleClosedException : DomainException
    {
        public RaffleClosedException(string raffleName)
            : base($"Raffle {raffleName} is already closed.")
        {
        }
    }
}
