import DashboardLayout from "../components/DashboardLayout.jsx"
import { Link } from "react-router-dom"

export default function Dashboard() {
  const tournaments = [
    { id: 1, name: "Summer Swim Meet", location: "Miami", startDate: "2025-09-10", endDate: "2025-09-12" },
    { id: 2, name: "Winter Championships", location: "Boston", startDate: "2025-12-01", endDate: "2025-12-03" },
  ].sort((a, b) => new Date(a.startDate) - new Date(b.startDate))

  const currentDate = new Date("2025-09-09T18:42:00+0530")

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
                  {tournaments.map((tournament) => {
                    const start = new Date(tournament.startDate)
                    const isTomorrow = start.toDateString() === new Date(currentDate.getTime() + 86400000).toDateString()
                    return (
                      <tr key={tournament.id}>
                        <td>{tournament.name} {isTomorrow && <span style={{ color: '#2ecc71' }}>(Starts Tomorrow)</span>}</td>
                        <td>{tournament.location}</td>
                        <td>{tournament.startDate}</td>
                        <td>{tournament.endDate}</td>
                        <td>
                          <Link to={`/universities/${tournament.id}`} className="btn ghost">
                            Universities
                          </Link>{" "}
                          <Link to={`/events/${tournament.id}`} className="btn ghost">
                            Events
                          </Link>{" "}
                          <Link to={`/players/${tournament.id}`} className="btn ghost">
                            All Players
                          </Link>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </DashboardLayout>
  )
}