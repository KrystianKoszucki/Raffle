namespace Raffle.Api.Exceptions
{
    public class DuplicateRaffleMemberException : DomainException
    {
        public DuplicateRaffleMemberException(string email)
            : base($"Member with email {email} already exists in this raffle.")
        {
        }
    }
}
