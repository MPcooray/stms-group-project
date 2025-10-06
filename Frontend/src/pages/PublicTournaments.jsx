import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import { listTournaments } from "../services/tournamentService.js";

export default function PublicTournaments() {
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
    <div className="public-page">
      {/* Header */}
      <header className="public-header">
        <div className="container">
          <div className="header-content">
            <Link to="/" className="brand">AquaChamps</Link>
            <nav className="public-nav">
              <Link to="/" className="nav-link">Home</Link>
              <Link to="/public/tournaments" className="nav-link active">Tournaments</Link>
            </nav>
            <Link to="/login" className="admin-login-btn" title="Admin Login">
              🔒
            </Link>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="public-main">
        <div className="container">
          <div className="page-header">
            <h2>All Tournaments</h2>
            <p>Select a tournament to view results and leaderboards</p>
          </div>

          {loading ? (
            <div className="loading-state">
              <p>Loading tournaments...</p>
            </div>
          ) : tournaments.length > 0 ? (
            <div className="tournaments-grid">
              {tournaments.map((tournament) => (
                <div key={tournament.id} className="tournament-card">
                  <div className="tournament-header">
                    <h3>{tournament.name}</h3>
                    <span className="tournament-date">
                      {tournament.startDate ? new Date(tournament.startDate).toLocaleDateString() : 'Date TBD'}
                    </span>
                  </div>
                  
                  <div className="tournament-details">
                    <p className="venue">
                      <span className="icon">📍</span>
                      {tournament.location || 'Venue TBD'}
                    </p>
                    {tournament.endDate && (
                      <p className="duration">
                        <span className="icon">📅</span>
                        {tournament.startDate ? new Date(tournament.startDate).toLocaleDateString() : 'Start TBD'} - {tournament.endDate ? new Date(tournament.endDate).toLocaleDateString() : 'End TBD'}
                      </p>
                    )}
                  </div>

                  <div className="tournament-actions">
                    <Link 
                      to={`/public/tournaments/${tournament.id}/results`} 
                      className="btn primary"
                    >
                      View Results
                    </Link>
                    <Link 
                      to={`/public/tournaments/${tournament.id}/leaderboard`} 
                      className="btn ghost"
                    >
                      Leaderboard
                    </Link>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="empty-state">
              <h3>No Tournaments Available</h3>
              <p>There are currently no tournaments to display.</p>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}