import { Link, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { listTournaments } from "../services/tournamentService.js";
import { getLeaderboard } from "../services/leaderboardService.js";
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

export default function PublicTournamentLeaderboard() {
  const { tournamentId } = useParams();
  const [tournament, setTournament] = useState(null);
  const [leaderboard, setLeaderboard] = useState({ players: [], universities: [] });
  const [loading, setLoading] = useState(true);
  const [activeView, setActiveView] = useState('players');

  useEffect(() => {
    const loadData = async () => {
      try {
        // Load tournament details
        const tournaments = await listTournaments();
        const foundTournament = tournaments.find(t => t.id == tournamentId);
        setTournament(foundTournament);

        if (foundTournament) {
          // Load leaderboard data
          const leaderboardData = await getLeaderboard(tournamentId);
          setLeaderboard(leaderboardData || { players: [], universities: [] });
        }
      } catch (error) {
        console.error("Failed to load leaderboard data:", error);
        setLeaderboard({ players: [], universities: [] });
      } finally {
        setLoading(false);
      }
    };

    if (tournamentId) {
      loadData();
    }
  }, [tournamentId]);

  const getRankClass = (rank) => {
    switch(rank) {
      case 1: return 'rank-gold';
      case 2: return 'rank-silver'; 
      case 3: return 'rank-bronze';
      default: return '';
    }
  };

  const getRankIcon = (rank) => {
    switch(rank) {
      case 1: return '🥇';
      case 2: return '🥈'; 
      case 3: return '🥉';
      default: return rank;
    }
  };

  if (loading) {
    return (
      <div className="public-page">
        <header className="public-header">
          <div className="container">
            <Link to="/" className="brand">AquaChamps</Link>
          </div>
        </header>
        <main className="public-main">
          <div className="container">
            <div className="loading-state">
              <p>Loading leaderboard...</p>
            </div>
          </div>
        </main>
      </div>
    );
  }

  if (!tournament) {
    return (
      <div className="public-page">
        <header className="public-header">
          <div className="container">
            <Link to="/" className="brand">AquaChamps</Link>
          </div>
        </header>
        <main className="public-main">
          <div className="container">
            <div className="empty-state">
              <h3>Tournament Not Found</h3>
              <Link to="/public/tournaments" className="btn primary">Back to Tournaments</Link>
            </div>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="public-page">
      {/* Header */}
      <header className="public-header">
        <div className="container">
          <div className="header-content">
            <Link to="/" className="brand">AquaChamps</Link>
            <nav className="public-nav">
              <Link to="/" className="nav-link">Home</Link>
              <Link to="/public/tournaments" className="nav-link">Tournaments</Link>
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
          {/* Tournament header */}
          <div className="tournament-header">
            <div className="tournament-info">
              <h1>{tournament.name} - Leaderboard</h1>
              <p className="tournament-meta">
                <span className="venue">📍 {tournament.location}</span>
                <span className="date">📅 {new Date(tournament.startDate).toLocaleDateString()}</span>
              </p>
            </div>
            <div className="tournament-actions">
              <Link 
                to={`/public/tournaments/${tournamentId}/results`} 
                className="btn primary"
              >
                View Results
              </Link>
              {/* Export button */}
              <button
                className="btn outline"
                onClick={async () => {
                  try {
                    const container = document.querySelector('.leaderboard-content');
                    if (!container) return alert('Leaderboard not visible to export.');

                    // use a white background for capture
                    const prevBg = container.style.backgroundColor;
                    container.style.backgroundColor = '#ffffff';

                    const canvas = await html2canvas(container, { scale: 2, useCORS: true });
                    const imgData = canvas.toDataURL('image/png');
                    const pdf = new jsPDF('p', 'mm', 'a4');

                    // Calculate dimensions to fit A4 while preserving aspect ratio
                    const pdfWidth = pdf.internal.pageSize.getWidth();
                    const pdfHeight = (canvas.height * pdfWidth) / canvas.width;

                    pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
                    pdf.save(`${tournament.name || 'leaderboard'}-leaderboard.pdf`);

                    container.style.backgroundColor = prevBg;
                  } catch (err) {
                    console.error('Export to PDF failed', err);
                    alert('Failed to export PDF. See console for details.');
                  }
                }}
              >
                📄 Export to PDF
              </button>
            </div>
          </div>

          {/* View toggle */}
          <div className="view-toggle">
            <button 
              className={`toggle-btn ${activeView === 'players' ? 'active' : ''}`}
              onClick={() => setActiveView('players')}
            >
              🏊 Player Rankings
            </button>
            <button 
              className={`toggle-btn ${activeView === 'universities' ? 'active' : ''}`}
              onClick={() => setActiveView('universities')}
            >
              🏫 University Rankings
            </button>
          </div>

          {/* Leaderboard content */}
          {activeView === 'players' && (
            <div className="leaderboard-content">
              <h2>Player Leaderboard</h2>
              {leaderboard.players && leaderboard.players.length > 0 ? (
                <div className="leaderboard-table-container">
                  <table className="leaderboard-table">
                    <thead>
                      <tr>
                        <th>Rank</th>
                        <th>Player</th>
                        <th>University</th>
                        <th>Total Points</th>
                      </tr>
                    </thead>
                    <tbody>
                      {leaderboard.players.map((player, index) => (
                        <tr key={player.id} className={getRankClass(index + 1)}>
                          <td className="rank-cell">
                            <span className="rank-display">
                              {getRankIcon(index + 1)}
                            </span>
                          </td>
                          <td className="player-name">{player.name}</td>
                          <td className="university">{player.university}</td>
                          <td className="points">{player.totalPoints}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="empty-state">
                  <h3>No Player Rankings Available</h3>
                  <p>Player rankings will appear here once results are recorded.</p>
                </div>
              )}
            </div>
          )}

          {activeView === 'universities' && (
            <div className="leaderboard-content">
              <h2>University Leaderboard</h2>
              {leaderboard.universities && leaderboard.universities.length > 0 ? (
                <div className="leaderboard-table-container">
                  <table className="leaderboard-table">
                    <thead>
                      <tr>
                        <th>Rank</th>
                        <th>University</th>
                        <th>Total Points</th>
                      </tr>
                    </thead>
                    <tbody>
                      {leaderboard.universities.map((university, index) => (
                        <tr key={university.id} className={getRankClass(index + 1)}>
                          <td className="rank-cell">
                            <span className="rank-display">
                              {getRankIcon(index + 1)}
                            </span>
                          </td>
                          <td className="university-name">{university.name}</td>
                          <td className="points">{university.totalPoints}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="empty-state">
                  <h3>No University Rankings Available</h3>
                  <p>University rankings will appear here once results are recorded.</p>
                </div>
              )}
            </div>
          )}

          {/* Points explanation */}
          <div className="points-explanation">
            <h3>Points System</h3>
            <p>Points are awarded based on finishing position in each event:</p>
            <div className="points-grid">
              <div className="point-item">🥇 1st: 10 points</div>
              <div className="point-item">🥈 2nd: 8 points</div>
              <div className="point-item">🥉 3rd: 7 points</div>
              <div className="point-item">4th: 5 points</div>
              <div className="point-item">5th: 4 points</div>
              <div className="point-item">6th: 3 points</div>
              <div className="point-item">7th: 2 points</div>
              <div className="point-item">8th: 1 point</div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}