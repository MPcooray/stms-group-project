namespace STMS.Api.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int UniversityId { get; set; }
        public University? University { get; set; }
        public int? Age { get; set; }
    public string? Gender { get; set; } // e.g., "Male", "Female"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
