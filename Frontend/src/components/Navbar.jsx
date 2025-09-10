"use client"

import { useAuth } from "../context/AuthContext"

const Navbar = () => {
  const { user, logout } = useAuth()

  const handleLogout = () => {
    logout()
  }

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <h2>STMS</h2>
      </div>
      <div className="navbar-user">
        <span>Welcome, {user?.email}</span>
        <button onClick={handleLogout} className="btn btn-outline">
          Logout
        </button>
      </div>
    </nav>
  )
}

export default Navbar