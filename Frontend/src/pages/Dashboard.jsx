import { useAuth } from '../context/AuthContext.jsx'

export default function Dashboard(){
  const { user, logout } = useAuth()
  return (
    <div className="container">
      <div className="card">
        <h2>Welcome, {user?.email || 'Admin'} 👋</h2>
        <p className="muted">Use the sidebar to manage tournaments and players.</p>
        <div className="space"></div>
        <button className="btn ghost" onClick={logout}>Logout</button>
      </div>
    </div>
  )
}
