import { Link, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { listTournaments } from "../services/tournamentService.js";
import { listEventsByTournament } from "../services/eventService.js";
import { getEventResults } from "../services/resultsService.js";
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

export default function PublicTournamentResults() {
  const { tournamentId } = useParams();
  const [tournament, setTournament] = useState(null);
  const [events, setEvents] = useState([]);
  const [results, setResults] = useState({});
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('results');

  useEffect(() => {
    const loadData = async () => {
      try {
        // Load tournament details
        const tournaments = await listTournaments();
        const foundTournament = tournaments.find(t => t.id == tournamentId);
        setTournament(foundTournament);

        if (foundTournament) {
          // Load events for this tournament
          const eventsData = await listEventsByTournament(tournamentId);
          setEvents(Array.isArray(eventsData) ? eventsData : []);

          // Load results for each event
          if (eventsData && eventsData.length > 0) {
            const resultsPromises = eventsData.map(event => 
              getEventResults(event.id).catch(() => [])
            );
            const resultsArray = await Promise.all(resultsPromises);
            
            const resultsObj = {};
            eventsData.forEach((event, index) => {
              resultsObj[event.id] = resultsArray[index] || [];
            });
            setResults(resultsObj);
          }
        }
      } catch (error) {
        console.error("Failed to load tournament data:", error);
      } finally {
        setLoading(false);
      }
    };

    if (tournamentId) {
      loadData();
    }
  }, [tournamentId]);

  const formatTiming = (ms) => {
    if (typeof ms !== "number" || ms <= 0) return "-";
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    const hundredths = Math.floor((ms % 1000) / 10);
    
    if (minutes > 0) {
      return `${minutes}:${seconds.toString().padStart(2, '0')}.${hundredths.toString().padStart(2, '0')}`;
    }
    return `${seconds}.${hundredths.toString().padStart(2, '0')}`;
  };

  const getRankClass = (rank) => {
    switch(rank) {
      case 1: return 'rank-gold';
      case 2: return 'rank-silver'; 
      case 3: return 'rank-bronze';
      default: return '';
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
              <p>Loading tournament results...</p>
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
              <h1>{tournament.name}</h1>
              <p className="tournament-meta">
                <span className="venue">📍 {tournament.location}</span>
                <span className="date">📅 {new Date(tournament.startDate).toLocaleDateString()}</span>
              </p>
            </div>
            <div className="tournament-actions">
              <Link 
                to={`/public/tournaments/${tournamentId}/leaderboard`} 
                className="btn primary"
              >
                View Leaderboard
              </Link>
              <button
                className="btn outline"
                onClick={async () => {
                  try {
                    const container = document.querySelector('.results-content');
                    if (!container) return alert('Results not visible to export.');

                    const prevBg = container.style.backgroundColor;
                    container.style.backgroundColor = '#ffffff';

                    const scale = 2;
                    const canvas = await html2canvas(container, { scale, useCORS: true });
                    const imgData = canvas.toDataURL('image/png');

                    const pdf = new jsPDF('p', 'mm', 'a4');
                    const pdfWidth = pdf.internal.pageSize.getWidth();
                    const pdfHeight = pdf.internal.pageSize.getHeight();

                    const pxPerMm = canvas.width / pdfWidth;
                    const pageHeightPx = Math.floor(pdfHeight * pxPerMm);

                    if (canvas.height <= pageHeightPx) {
                      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, (canvas.height / pxPerMm));
                    } else {
                      let remainingHeight = canvas.height;
                      let position = 0;
                      while (remainingHeight > 0) {
                        const pageCanvas = document.createElement('canvas');
                        pageCanvas.width = canvas.width;
                        pageCanvas.height = Math.min(pageHeightPx, remainingHeight);
                        const ctx = pageCanvas.getContext('2d');
                        ctx.fillStyle = '#ffffff';
                        ctx.fillRect(0, 0, pageCanvas.width, pageCanvas.height);
                        ctx.drawImage(canvas, 0, position, canvas.width, pageCanvas.height, 0, 0, pageCanvas.width, pageCanvas.height);

                        const pageData = pageCanvas.toDataURL('image/png');
                        const pageScaledHeightMm = pageCanvas.height / pxPerMm;

                        if (position > 0) pdf.addPage();
                        pdf.addImage(pageData, 'PNG', 0, 0, pdfWidth, pageScaledHeightMm);

                        remainingHeight -= pageCanvas.height;
                        position += pageCanvas.height;
                      }
                    }

                    pdf.save(`${(tournament && tournament.name) ? tournament.name.replace(/[^a-z0-9-_ ]/gi,'') : 'results'}-results.pdf`);
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

          {/* Tabs */}
          <div className="tabs">
            <button 
              className={`tab ${activeTab === 'results' ? 'active' : ''}`}
              onClick={() => setActiveTab('results')}
            >
              Event Results
            </button>
            <button 
              className={`tab ${activeTab === 'overview' ? 'active' : ''}`}
              onClick={() => setActiveTab('overview')}
            >
              Overview
            </button>
          </div>

          {/* Tab content */}
          {activeTab === 'results' && (
            <div className="results-content">
              {events.length > 0 ? (
                <div className="events-results">
                  {events.map((event) => (
                    <div key={event.id} className="event-results-card">
                      <h3 className="event-title">{event.name}</h3>
                      
                      {results[event.id] && results[event.id].length > 0 ? (
                        <div className="results-table-container">
                          <table className="results-table">
                            <thead>
                              <tr>
                                <th>Rank</th>
                                <th>Player</th>
                                <th>University</th>
                                <th>Time</th>
                                <th>Points</th>
                              </tr>
                            </thead>
                            <tbody>
                              {results[event.id].map((result, index) => (
                                <tr key={`${result.playerId}-${result.eventId}`} className={getRankClass(index + 1)}>
                                  <td className="rank-cell">
                                    <span className="rank">{index + 1}</span>
                                    {index < 3 && <span className="medal">🏅</span>}
                                  </td>
                                  <td className="player-name">{result.playerName}</td>
                                  <td className="university">{result.universityName}</td>
                                  <td className="time">{formatTiming(result.timeMs)}</td>
                                  <td className="points">{result.points || '-'}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      ) : (
                        <p className="no-results">No results available for this event yet.</p>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <div className="empty-state">
                  <h3>No Events Available</h3>
                  <p>This tournament doesn't have any events configured yet.</p>
                </div>
              )}
            </div>
          )}

          {activeTab === 'overview' && (
            <div className="overview-content">
              <div className="overview-stats">
                <div className="stat-card">
                  <h4>{events.length}</h4>
                  <p>Total Events</p>
                </div>
                <div className="stat-card">
                  <h4>{Object.values(results).flat().length}</h4>
                  <p>Total Results</p>
                </div>
                <div className="stat-card">
                  <h4>{tournament.location}</h4>
                  <p>Venue</p>
                </div>
              </div>
              
              <div className="tournament-description">
                <h3>Tournament Information</h3>
                <p><strong>Name:</strong> {tournament.name}</p>
                <p><strong>Venue:</strong> {tournament.location}</p>
                <p><strong>Date:</strong> {new Date(tournament.startDate).toLocaleDateString()}</p>
                {tournament.endDate && (
                  <p><strong>End Date:</strong> {new Date(tournament.endDate).toLocaleDateString()}</p>
                )}
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}