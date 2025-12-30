namespace Raffle.Api.Exceptions
{
    public class NoRaffleMembersException : DomainException
    {
        public NoRaffleMembersException(string raffleName)
            : base($"Raffle '{raffleName}' has no members.")
        {
        }
    }
}
