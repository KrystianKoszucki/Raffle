using FluentValidation;
using Raffle.Api.Contracts;

namespace Raffle.Api.Validation
{
    public class EnterRaffleDrawRequestValidator : AbstractValidator<EnterRaffleDrawRequest>
    {
        public EnterRaffleDrawRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
