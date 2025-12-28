using System.ComponentModel.DataAnnotations;

namespace Raffle.Api.Contracts
{
    public record EnterRaffleDrawRequest(
        [param: Required]
        [param: MinLength(1)]
        string Name,

        [param: Required]
        [param: EmailAddress]
        string Email);
}
