import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { listTournaments } from "../services/tournamentService.js";
import { listEventsByTournament } from "../services/eventService.js";
import { getEventResults } from "../services/resultsService.js";
import DashboardLayout from "../components/DashboardLayout.jsx";

export default function Results() {
  const { tournamentId } = useParams();
  const [tournaments, setTournaments] = useState([]);
  const [selectedTournament, setSelectedTournament] = useState(null);
  const [events, setEvents] = useState([]);
  const [results, setResults] = useState({});
  const [status, setStatus] = useState("");

  useEffect(() => {
    listTournaments()
      .then(data => setTournaments(Array.isArray(data) ? data : []))
      .catch(() => setStatus("Failed to load tournaments"));
  }, []);

  useEffect(() => {
    if (selectedTournament) {
      listEventsByTournament(selectedTournament.id)
        .then(data => setEvents(Array.isArray(data) ? data : []))
        .catch(() => setStatus("Failed to load events"));
    }
  }, [selectedTournament]);

  useEffect(() => {
    if (selectedTournament && events.length > 0) {
      Promise.all(events.map(ev => getEventResults(ev.id)))
        .then(resArr => {
          const resObj = {};
          events.forEach((ev, idx) => {
            resObj[ev.id] = resArr[idx];
          });
          setResults(resObj);
        })
        .catch(() => setStatus("Failed to load results"));
    }
  }, [selectedTournament, events]);

  function formatTiming(ms) {
    if (typeof ms !== "number" || ms < 0) return "-";
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    const hundredths = Math.floor((ms % 1000) / 10);
    if (minutes > 0) {
      return `${minutes}:${seconds.toString().padStart(2, "0")}.${hundredths.toString().padStart(2, "0")}`;
    } else {
      return `${seconds}.${hundredths.toString().padStart(2, "0")}`;
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Event Results</h2>
          {status && <div className="error">{status}</div>}
          <div style={{ marginBottom: 24 }}>
            <label htmlFor="tournament-select">Select Tournament:</label>
            <select
              id="tournament-select"
              value={selectedTournament?.id || ""}
              onChange={e => {
                const t = tournaments.find(t => t.id === Number(e.target.value));
                setSelectedTournament(t || null);
                setEvents([]);
                setResults({});
              }}
              style={{ marginLeft: 12 }}
            >
              <option value="">-- Select --</option>
              {tournaments.map(t => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
          </div>
          {selectedTournament && events.length > 0 && (
            <div>
              {events.map(ev => {
                const eventResults = results[ev.id] || [];
                // Sort: valid timings first, then N/A
                const validResults = eventResults.filter(r => r.timeMs > 0).sort((a, b) => a.timeMs - b.timeMs);
                const naResults = eventResults.filter(r => r.timeMs === 0);
                return (
                  <div key={ev.id} style={{ marginBottom: 32 }}>
                    <h3>{ev.name}</h3>
                    <table>
                      <thead>
                        <tr>
                          <th>Rank</th>
                          <th>Player</th>
                          <th>University</th>
                          <th>Timing</th>
                          <th>Points</th>
                        </tr>
                      </thead>
                      <tbody>
                        {validResults.map((r) => (
                          <tr key={r.playerId}>
                            <td>{r.rank ?? '-'}</td>
                            <td>{r.playerName}</td>
                            <td>{r.universityName}</td>
                            <td>{formatTiming(r.timeMs)}</td>
                            <td>{r.points}</td>
                          </tr>
                        ))}
                        {naResults.map((r, idx) => (
                          <tr key={r.playerId}>
                            <td>-</td>
                            <td>{r.playerName}</td>
                            <td>{r.universityName}</td>
                            <td style={{ color: '#ffa500' }}>N/A</td>
                            <td>-</td>
                          </tr>
                        ))}
                        {eventResults.length === 0 && (
                          <tr>
                            <td colSpan="5" className="muted">No results yet.</td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </DashboardLayout>
  );
}
