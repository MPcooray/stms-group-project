namespace STMS.Api.Models
{
    public class Timing
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public string Event { get; set; } = "";
        public int TimeMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
