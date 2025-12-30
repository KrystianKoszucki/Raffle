namespace Raffle.Api.Exceptions
{
    public class RaffleDrawAlreadyExistsException : DomainException
    {
        public RaffleDrawAlreadyExistsException(string raffleName) : base($"RaffleDraw '{raffleName}' already exists.")
        {
        }
    }
}
