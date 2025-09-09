namespace STMS.Api.Models
{
    public class PlayerEvent
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        // e.g., "100m Freestyle", "200m IM"
        public string Event { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
