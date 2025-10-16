import { useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import { getLeaderboard } from "../services/leaderboardService.js"
import { listTournaments } from "../services/tournamentService.js"
import { Link } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { listEventsByTournament } from "../services/eventService.js"
import { listPlayersByTournament } from "../services/playerService.js"
import { listRegistrations } from "../services/tournamentEventRegistrationsService.js"
import { getTiming } from "../services/timingService.js"

// Same point system as EventTimings
function getPoints(rank) {
  switch (rank) {
    case 1: return 10;
    case 2: return 8;
    case 3: return 7;
    case 4: return 5;
    case 5: return 4;
    case 6: return 3;
    case 7: return 2;
    case 8: return 1;
    default: return 0;
  }
}

export default function Leaderboard() {
  const { tournamentId } = useParams();
  const [tournaments, setTournaments] = useState([]);
  const [status, setStatus] = useState("");
  const [playerLeaderboard, setPlayerLeaderboard] = useState([]);
  const [universityLeaderboard, setUniversityLeaderboard] = useState([]);

  useEffect(() => {
    if (tournamentId) {
      setStatus("");
      getLeaderboard(tournamentId)
        .then(data => {
          setPlayerLeaderboard(Array.isArray(data.players) ? data.players : []);
          setUniversityLeaderboard(Array.isArray(data.universities) ? data.universities : []);
        })
        .catch(() => setStatus("Failed to load leaderboard"));
    } else {
      setStatus("");
      listTournaments()
        .then(data => setTournaments(Array.isArray(data) ? data : []))
        .catch(() => setStatus("Failed to load tournaments"));
    }
  }, [tournamentId]);

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          {!tournamentId ? (
            <>
              <h2 style={{ marginBottom: 24 }}>Leaderboards</h2>
              {status && <div className="error">{status}</div>}
              <div style={{
                display: "grid",
                gridTemplateColumns: "repeat(auto-fit, minmax(320px, 1fr))",
                gap: "24px",
                marginTop: "16px"
              }}>
                {tournaments.map((t) => (
                  <div
                    key={t.id}
                    className="card"
                    style={{
                      minHeight: 160,
                      display: "flex",
                      flexDirection: "column",
                      justifyContent: "space-between",
                      alignItems: "flex-start",
                      boxShadow: "0 2px 12px rgba(46,160,255,0.08)",
                      transition: "transform 0.15s"
                    }}
                    onMouseEnter={e => e.currentTarget.style.transform = "scale(1.03)"}
                    onMouseLeave={e => e.currentTarget.style.transform = "scale(1)"}
                  >
                    <div>
                      <h3 style={{ marginBottom: 8 }}>{t.name}</h3>
                      <p className="muted" style={{ marginBottom: 16 }}>{t.location}</p>
                    </div>
                    <Link to={`/leaderboard/${t.id}`} className="btn primary" style={{ alignSelf: "flex-end", marginTop: 8 }}>
                      View Leaderboard
                    </Link>
                  </div>
                ))}
                {tournaments.length === 0 && (
                  <div className="muted">No tournaments found.</div>
                )}
              </div>
            </>
          ) : (
            <>
              <h2 style={{ marginBottom: 24 }}>Leaderboard</h2>
              {status && <div className="error">{status}</div>}
              <div style={{ display: "flex", gap: "48px", flexWrap: "wrap" }}>
                <div style={{ flex: 1, minWidth: 320 }}>
                  <h3>Player Leaderboard</h3>
                  <table>
                    <thead>
                      <tr>
                        <th>Rank</th>
                        <th>Name</th>
                        <th>University</th>
                        <th>Total Points</th>
                      </tr>
                    </thead>
                    <tbody>
                      {playerLeaderboard.map((p) => (
                        <tr key={p.id}>
                          <td>{p.rank}</td>
                          <td>{p.name}</td>
                          <td>{p.university}</td>
                          <td>{p.totalPoints}</td>
                        </tr>
                      ))}
                      {playerLeaderboard.length === 0 && (
                        <tr>
                          <td colSpan="4" className="muted">No results yet.</td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
                <div style={{ flex: 1, minWidth: 320 }}>
                  <h3>University Leaderboard</h3>
                  <table>
                    <thead>
                      <tr>
                        <th>Rank</th>
                        <th>University</th>
                        <th>Total Points</th>
                      </tr>
                    </thead>
                    <tbody>
                      {universityLeaderboard.map((u) => (
                        <tr key={u.id}>
                          <td>{u.rank}</td>
                          <td>{u.name}</td>
                          <td>{u.totalPoints}</td>
                        </tr>
                      ))}
                      {universityLeaderboard.length === 0 && (
                        <tr>
                          <td colSpan="3" className="muted">No results yet.</td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            </>
          )}
        </div>
      </div>
    </DashboardLayout>
  );
}
