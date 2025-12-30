namespace Raffle.Api.Exceptions
{
    public class NoMembersProvidedException : DomainException
    {
        public NoMembersProvidedException() : base("No members were provided.")
        {
        }
    }
}
