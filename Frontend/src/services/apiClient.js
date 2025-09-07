import axios from "axios"

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5287",
})

api.interceptors.request.use((cfg) => {
  const t = localStorage.getItem("token")
  if (t) cfg.headers.Authorization = `Bearer ${t}`
  return cfg
})

export default api
