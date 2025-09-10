using System.ComponentModel.DataAnnotations;

namespace STMS.Api.Models
{
    public class TournamentEvent
    {
        public int Id { get; set; }

        [Required]
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = string.Empty; // e.g., "100m Freestyle"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
