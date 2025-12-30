using FluentValidation;
using Raffle.Api.Contracts;

namespace Raffle.Api.Validation
{
    public class CreateRaffleDrawRequestValidator : AbstractValidator<CreateRaffleDrawRequest>
    {
        public CreateRaffleDrawRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(100);
        }
    }
}
