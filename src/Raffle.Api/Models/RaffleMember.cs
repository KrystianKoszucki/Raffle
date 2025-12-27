using System.ComponentModel.DataAnnotations;

namespace Raffle.Api.Models
{
    public class RaffleMember
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RaffleDrawId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Raffle cannot be empty")]
        public RaffleDraw RaffleDraw { get; set; }
    }
}
