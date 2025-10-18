import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { useEffect, useMemo, useState } from "react";
import { Link, useLocation, useNavigate, useParams } from "react-router-dom";
import { getLeaderboard } from "../services/leaderboardService.js";
import { listTournaments } from "../services/tournamentService.js";

export default function PublicTournamentLeaderboard() {
  const { tournamentId } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const [tournament, setTournament] = useState(null);
  const [leaderboard, setLeaderboard] = useState({ players: [], universities: [] });
  const [loading, setLoading] = useState(true);
  const [activeView, setActiveView] = useState('players');
  const genderFromUrl = useMemo(() => new URLSearchParams(location.search).get('gender') || 'All', [location.search]);
  const [gender, setGender] = useState(genderFromUrl);

  useEffect(() => {
    const loadData = async () => {
      try {
        // Load tournament details
        const tournaments = await listTournaments();
        const foundTournament = tournaments.find(t => t.id == tournamentId);
        setTournament(foundTournament);

        if (foundTournament) {
          // Load leaderboard data
          const leaderboardData = await getLeaderboard(tournamentId, gender === 'All' ? undefined : gender);
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
  }, [tournamentId, gender]);

  // Keep URL query in sync when gender changes
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    if (gender && gender !== 'All') params.set('gender', gender);
    else params.delete('gender');
    const search = params.toString();
    const newUrl = `${location.pathname}${search ? `?${search}` : ''}`;
    if (newUrl !== `${location.pathname}${location.search}`) {
      navigate(newUrl, { replace: true });
    }
  }, [gender]);

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
                onClick={() => {
                  try {
                    const pdf = new jsPDF('p', 'mm', 'a4');
                    const title = `${tournament?.name || 'Leaderboard'} - Leaderboard`;
                    pdf.setFontSize(14);
                    pdf.text(title, 14, 16);

                    if (activeView === 'players') {
                      const head = [['Rank', 'Player', 'University', 'Total Points']];
                      const body = leaderboard.players.map((p) => [p.rank || '-', p.name || '-', p.university || '-', p.totalPoints ?? '-']);
                      autoTable(pdf, {
                        head,
                        body,
                        startY: 22,
                        styles: { fontSize: 10 },
                        headStyles: { fillColor: [30, 30, 30], textColor: 255 },
                        alternateRowStyles: { fillColor: [245, 245, 245] },
                        margin: { left: 14, right: 14 }
                      });
                    } else {
                      const head = [['Rank', 'University', 'Total Points']];
                      const body = leaderboard.universities.map((u) => [u.rank || '-', u.name || '-', u.totalPoints ?? '-']);
                      autoTable(pdf, {
                        head,
                        body,
                        startY: 22,
                        styles: { fontSize: 10 },
                        headStyles: { fillColor: [30, 30, 30], textColor: 255 },
                        alternateRowStyles: { fillColor: [245, 245, 245] },
                        margin: { left: 14, right: 14 }
                      });
                    }

                    pdf.save(`${(tournament && tournament.name) ? tournament.name.replace(/[^a-z0-9-_ ]/gi,'') : 'leaderboard'}-leaderboard.pdf`);
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

          {/* Gender selector */}
          <div className="view-toggle" style={{ marginTop: '1rem' }}>
            <button className={`toggle-btn ${gender === 'All' ? 'active' : ''}`} onClick={() => setGender('All')}>All</button>
            <button className={`toggle-btn ${gender === 'Male' ? 'active' : ''}`} onClick={() => setGender('Male')}>Male</button>
            <button className={`toggle-btn ${gender === 'Female' ? 'active' : ''}`} onClick={() => setGender('Female')}>Female</button>
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
                      {leaderboard.players.map((player) => (
                        <tr key={player.id} className={getRankClass(player.rank)}>
                          <td className="rank-cell">
                            <span className="rank-display">
                              {getRankIcon(player.rank)}
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
                      {leaderboard.universities.map((university) => (
                        <tr key={university.id} className={getRankClass(university.rank)}>
                          <td className="rank-cell">
                            <span className="rank-display">
                              {getRankIcon(university.rank)}
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