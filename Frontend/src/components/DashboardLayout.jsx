
import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext.jsx";
import { Link, useLocation } from "react-router-dom";

export default function DashboardLayout({ children }) {
  const { user, logout } = useAuth()
  const location = useLocation()

  const menuItems = [
  { path: "/dashboard", label: "Dashboard", icon: "📊" },
  { path: "/tournaments", label: "Tournaments", icon: "🏆" },
  { path: "/leaderboard", label: "Leaderboard", icon: "🥇" },
  { path: "/results", label: "Results", icon: "📋" },
  ];

  return (
    <div className="dashboard-layout">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <h2 className="brand">STMS</h2>
        </div>
        <nav className="sidebar-nav">
          {menuItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={`sidebar-link ${location.pathname === item.path ? "active" : ""}`}
            >
              <span className="sidebar-icon">{item.icon}</span>
              <span className="sidebar-label">{item.label}</span>
            </Link>
          ))}
        </nav>
      </aside>

      {/* Main Content */}
      <div className="main-content">
        {/* Navbar */}
        <nav className="navbar">
          <div className="container">
            <div className="navbar-user">
              <span>Welcome, {user?.email || "Admin"}</span>
              <button onClick={logout} className="btn ghost">
                Logout
              </button>
            </div>
          </div>
        </nav>

        {/* Content Area */}
        <div className="content-area">{children}</div>
      </div>
    </div>
  )
}
