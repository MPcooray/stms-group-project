"use client"

import { useState } from "react"
import { useNavigate, useLocation } from "react-router-dom"
import { useAuth } from "../context/AuthContext.jsx"
import { email as emailValidator, required } from "../utils/validators.js"

export default function Login() {
  const [email, setEmail] = useState("admin@stms.com")
  const [password, setPassword] = useState("Admin#123")
  const [error, setError] = useState("")
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const location = useLocation()
  const { login } = useAuth()
  const from = location.state?.from?.pathname || "/dashboard"

  const handleSubmit = async (e) => {
    e.preventDefault()
    const emailErr = emailValidator(email)
    const passErr = required(password)
    if (emailErr || passErr) {
      setError(emailErr || passErr)
      return
    }
    setLoading(true)
    setError("")
    try {
      await login(email, password)
      navigate(from, { replace: true })
    } catch (err) {
      setError(err?.response?.data?.message || "Login failed")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container" style={{ maxWidth: 480, marginTop: 80 }}>
      <div className="card">
        <h2>Admin </h2>
        <p className="muted">Sign in to manage STMS.</p>
        <div className="space"></div>
        <form onSubmit={handleSubmit}>
          <label>Email</label>
          <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="admin@stms.com" />
          <div className="space"></div>
          <label>Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
          />
          <div className="space"></div>
          {error && <div className="error">{error}</div>}
          <div className="space"></div>
          <button id="loginButton" className="btn primary" disabled={loading}>
            {loading ? "Signing in…" : "Sign In"}
          </button>
        </form>
      </div>
    </div>
  )
}
