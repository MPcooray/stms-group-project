"use client"

import { createContext, useContext, useEffect, useState } from "react"
import { getProfile, login as loginApi, logout as logoutApi } from "../services/authService.js"

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(localStorage.getItem("token"))

  useEffect(() => {
    if (token) {
      getProfile()
        .then((u) => setUser(u))
        .catch(() => {
          setUser(null)
          setToken(null)
          localStorage.removeItem("token")
        })
    }
  }, [token])

  const login = async (email, password) => {
    const { token } = await loginApi(email, password)
    localStorage.setItem("token", token)
    setToken(token)
    const profile = await getProfile().catch(() => ({ email }))
    setUser(profile)
  }

  const logout = () => {
    logoutApi()
    localStorage.removeItem("token")
    setToken(null)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, token, isAuthenticated: !!token, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext)
