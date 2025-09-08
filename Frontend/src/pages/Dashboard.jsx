import DashboardLayout from "../components/DashboardLayout.jsx"
import { Link } from "react-router-dom"

export default function Dashboard() {
  const tournaments = [
    { id: 1, name: "Summer Swim Meet", location: "Miami", startDate: "2025-09-10", endDate: "2025-09-12" },
    { id: 2, name: "Winter Championships", location: "Boston", startDate: "2025-12-01", endDate: "2025-12-03" },
  ]

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Dashboard Overview</h2>
          <p className="muted">Welcome to the Swimming Tournament Management System.</p>
          <div className="space"></div>
          <div className="row">
            <div className="card" style={{ flex: 1 }}>
              <h3>Select Tournament</h3>
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Location</th>
                    <th>Start</th>
                    <th>End</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {tournaments.map((tournament) => (
                    <tr key={tournament.id}>
                      <td>{tournament.name}</td>
                      <td>{tournament.location}</td>
                      <td>{tournament.startDate}</td>
                      <td>{tournament.endDate}</td>
                      <td>
                        <Link to={`/universities/${tournament.id}`} className="btn ghost">
                          Universities
                        </Link>{" "}
                        <Link to={`/events/${tournament.id}`} className="btn ghost">
                          Events
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="card" style={{ flex: 1 }}>
              <Link to="/tournaments" className="btn primary">
                Add Tournament
              </Link>
            </div>
          </div>
        </div>
      </div>
    </DashboardLayout>
  )
}