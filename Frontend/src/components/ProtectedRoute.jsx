import { useAuth } from '../context/AuthContext.jsx'
import { Navigate, useLocation } from 'react-router-dom'

export default function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth()
  const location = useLocation()
  if (!isAuthenticated) return <Navigate to="/login" replace state={{ from: location }} />
  return children
}
