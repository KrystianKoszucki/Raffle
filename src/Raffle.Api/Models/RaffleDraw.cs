using System.ComponentModel.DataAnnotations;

namespace Raffle.Api.Models
{
    public class RaffleDraw
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public Guid? WinnerMemberId { get; set; }
        public RaffleMember? WinnerMember { get; set; }
        public List<RaffleMember> RaffleMembers { get; set; } = new();
    }
}
