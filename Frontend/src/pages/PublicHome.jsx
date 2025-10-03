import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import { listTournaments } from "../services/tournamentService.js";

export default function PublicHome() {
  const [tournaments, setTournaments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadTournaments = async () => {
      try {
        const data = await listTournaments();
        setTournaments(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error("Failed to load tournaments:", error);
        setTournaments([]);
      } finally {
        setLoading(false);
      }
    };

    loadTournaments();
  }, []);

  return (
    <div className="public-home">
      {/* Header with padlock */}
      <header className="public-header">
        <div className="container">
          <div className="header-content">
            <h1 className="brand">Swimming Tournament Management System</h1>
            <Link to="/login" className="admin-login-btn" title="Admin Login">
              🔒
            </Link>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="public-main">
        <div className="container">
          <div className="hero-section">
            <h2>Welcome to STMS</h2>
            <p>View live tournament results, leaderboards, and swimming competition data.</p>
          </div>

          {/* Tournament Statistics */}
          <div className="stats-section">
            <div className="stat-card">
              <h3>{tournaments.length}</h3>
              <p>Active Tournaments</p>
            </div>
            {/* <div className="stat-card">
              <h3>Live</h3>
              <p>Results Available</p>
            </div>
            <div className="stat-card">
              <h3>Real-time</h3>
              <p>Leaderboards</p>
            </div> */}
          </div>

          {/* Tournaments button */}
          <div className="action-section">
            <Link to="/public/tournaments" className="btn primary large">
              View Tournaments
            </Link>
          </div>

          {/* Recent tournaments preview */}
          <div className="tournaments-preview">
            <h3>Recent Tournaments</h3>
            {loading ? (
              <p className="muted">Loading tournaments...</p>
            ) : tournaments.length > 0 ? (
              <div className="tournament-grid">
                {tournaments.slice(0, 3).map((tournament) => (
                  <div key={tournament.id} className="tournament-preview-card">
                    <h4>{tournament.name}</h4>
                    <p className="muted">{tournament.venue || 'Venue TBD'}</p>
                    <p className="muted">
                      {tournament.date ? new Date(tournament.date).toLocaleDateString() : 'Date TBD'}
                    </p>
                    <Link 
                      to={`/public/tournaments/${tournament.id}/results`} 
                      className="btn ghost small"
                    >
                      View Results
                    </Link>
                  </div>
                ))}
              </div>
            ) : (
              <p className="muted">No tournaments available</p>
            )}
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="public-footer">
        <div className="container">
          <p>&copy; 2025 Swimming Tournament Management System</p>
        </div>
      </footer>
    </div>
  );
}