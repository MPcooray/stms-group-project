namespace STMS.Api.Models
{
    public class University
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
