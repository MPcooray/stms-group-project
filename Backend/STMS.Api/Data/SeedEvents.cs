using System;
using System.Linq;
using STMS.Api.Data;
using STMS.Api.Models;

namespace STMS.Api.Data
{
    public static class SeedEvents
    {
        public static void SeedTournamentEvents(StmsDbContext db)
        {
            string[] events = new[] {
                "50m Butterfly",
                "50m Backstroke",
                "50m Breaststroke",
                "50m freestyle",
                "100m Butterfly",
                "100m Backstroke",
                "100m Breaststroke",
                "100m freestyle",
                "200m Butterfly",
                "200m Backstroke",
                "200m Breaststroke",
                "200m freestyle",
                "400m freestyle",
                "200m Individual Medley",
                "400m Individual Medley"
            };

            var tournaments = db.Tournaments.ToList();
            foreach (var t in tournaments)
            {
                foreach (var name in events)
                {
                    if (!db.TournamentEvents.Any(e => e.TournamentId == t.Id && e.Name == name))
                    {
                        db.TournamentEvents.Add(new TournamentEvent
                        {
                            TournamentId = t.Id,
                            Name = name,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            db.SaveChanges();
        }

        // Call this after adding a new tournament
        public static void SeedEventsForTournament(StmsDbContext db, int tournamentId)
        {
            string[] events = new[] {
                "50m Butterfly",
                "50m Backstroke",
                "50m Breaststroke",
                "50m freestyle",
                "100m Butterfly",
                "100m Backstroke",
                "100m Breaststroke",
                "100m freestyle",
                "200m Butterfly",
                "200m Backstroke",
                "200m Breaststroke",
                "200m freestyle",
                "400m freestyle",
                "200m Individual Medley",
                "400m Individual Medley"
            };
            foreach (var name in events)
            {
                if (!db.TournamentEvents.Any(e => e.TournamentId == tournamentId && e.Name == name))
                {
                    db.TournamentEvents.Add(new TournamentEvent
                    {
                        TournamentId = tournamentId,
                        Name = name,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            db.SaveChanges();
        }
    }
}
