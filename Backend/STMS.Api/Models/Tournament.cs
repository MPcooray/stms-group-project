using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STMS.Api.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = "";

        [Required, MaxLength(120)]
        public string Venue { get; set; } = "";

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime? EndDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
