import DashboardLayout from "../components/DashboardLayout.jsx"

export default function Dashboard() {
  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Dashboard Overview</h2>
          <p className="muted">Welcome to the Swimming Tournament Management System.</p>
          <div className="space"></div>
          <div className="row">
            <div className="card" style={{ flex: 1 }}>
              <h3>Quick Stats</h3>
              <p className="muted">System statistics will appear here.</p>
            </div>
            <div className="card" style={{ flex: 1 }}>
              <h3>Recent Activity</h3>
              <p className="muted">Recent tournament and player activities.</p>
            </div>
          </div>
        </div>
      </div>
    </DashboardLayout>
  )
}
