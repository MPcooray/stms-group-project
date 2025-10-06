import { Navigate, Route, Routes } from "react-router-dom"
import { AuthProvider } from "./context/AuthContext.jsx"
import ProtectedRoute from "./components/ProtectedRoute.jsx"
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
import PublicHome from "./pages/PublicHome.jsx"
import PublicTournaments from "./pages/PublicTournaments.jsx"
import PublicTournamentResults from "./pages/PublicTournamentResults.jsx"
import PublicTournamentLeaderboard from "./pages/PublicTournamentLeaderboard.jsx"

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<PublicHome />} />
        <Route path="/public/tournaments" element={<PublicTournaments />} />
        <Route path="/public/tournaments/:tournamentId/results" element={<PublicTournamentResults />} />
        <Route path="/public/tournaments/:tournamentId/leaderboard" element={<PublicTournamentLeaderboard />} />
        
        {/* Admin login route */}
        <Route path="/login" element={<Login />} />
        
        {/* Protected admin routes */}
        <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
        <Route path="/players/:tournamentId" element={<ProtectedRoute><AllPlayers /></ProtectedRoute>} />
        <Route path="/universities/:tournamentId" element={<ProtectedRoute><Universities /></ProtectedRoute>} />
        <Route path="/universities/:tournamentId/:universityId/players" element={<ProtectedRoute><Players /></ProtectedRoute>} />
        <Route path="/tournaments" element={<ProtectedRoute><Tournaments /></ProtectedRoute>} />
        <Route path="/events/:tournamentId" element={<ProtectedRoute><Events /></ProtectedRoute>} />
        <Route path="/events/:tournamentId/:eventId/timings" element={<ProtectedRoute><EventTimings /></ProtectedRoute>} />
        <Route path="/leaderboard" element={<ProtectedRoute><Leaderboard /></ProtectedRoute>} />
        <Route path="/leaderboard/:tournamentId" element={<ProtectedRoute><Leaderboard /></ProtectedRoute>} />
        <Route path="/results" element={<ProtectedRoute><Results /></ProtectedRoute>} />
        
        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AuthProvider>
  )
}
