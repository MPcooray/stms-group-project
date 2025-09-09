import { Link, useLocation } from "react-router-dom"

const Sidebar = () => {
  const location = useLocation()

  const menuItems = [
    { path: "/dashboard", label: "Dashboard", icon: "📊" },
    { path: "/tournaments", label: "Tournaments", icon: "🏆" },
    {/*
      { path: "/players/1", label: "Players", icon: "🏊‍♂️" }, // Redirect to /players/1 (Summer Swim Meet) 
    */}
  ]

  return (
    <aside className="sidebar">
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
  )
}

export default Sidebar