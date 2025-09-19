import { Navigate, Route, Routes } from "react-router-dom"
import { AuthProvider } from "./context/AuthContext.jsx"
import AllPlayers from "./pages/AllPlayers.jsx"
import Leaderboard from "./pages/Leaderboard.jsx"
import Dashboard from "./pages/Dashboard.jsx"
import Events from "./pages/Events.jsx"
import EventTimings from "./pages/EventTimings.jsx"
import Login from "./pages/Login.jsx"
import Players from "./pages/Players.jsx"
import Tournaments from "./pages/Tournaments.jsx"
import Universities from "./pages/Universities.jsx"
import Results from "./pages/Results.jsx"

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/players/:tournamentId" element={<AllPlayers />} />
        <Route path="/universities/:tournamentId" element={<Universities />} />
        <Route path="/universities/:tournamentId/:universityId/players" element={<Players />} />
        <Route path="/tournaments" element={<Tournaments />} />
        <Route path="/events/:tournamentId" element={<Events />} />
        <Route path="/events/:tournamentId/:eventId/timings" element={<EventTimings />} />
  <Route path="/leaderboard" element={<Leaderboard />} />
  <Route path="/leaderboard/:tournamentId" element={<Leaderboard />} />
  <Route path="/results" element={<Results />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </AuthProvider>
  )
}
