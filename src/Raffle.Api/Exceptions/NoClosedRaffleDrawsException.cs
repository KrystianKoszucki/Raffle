namespace Raffle.Api.Exceptions
{
    public class NoClosedRaffleDrawsException : DomainException
    {
        public NoClosedRaffleDrawsException() : base("No raffle draw has been closed.")
        {
        }
    }
}
