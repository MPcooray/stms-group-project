import api from "./apiClient.js"

export async function login(email, password) {
  const res = await api.post("/auth/login", { email, password })
  const token = res.data?.token ?? res.data?.data?.token
  if (!token) throw new Error("No token returned from server")
  return { token }
}

export async function getProfile() {
  try {
    const res = await api.get("/auth/me")
    return res.data
  } catch {
    // fallback if /auth/me not implemented yet
    return { email: "admin@stms.com", role: "Admin" }
  }
}

export function logout() {
  return true
}
