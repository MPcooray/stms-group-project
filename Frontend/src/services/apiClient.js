import axios from "axios"

// Prefer runtime-injected API base (config.js -> window.__API_BASE__),
// then Vite build-time env var, then local fallback. This let the same
// static build work across environments when `config.js` is created at
// container startup by the nginx entrypoint.
function getApiBase() {
  try {
    // runtime config created at /usr/share/nginx/html/config.js
    // which sets window.__API_BASE__
    if (typeof window !== "undefined" && window.__API_BASE__) {
      return window.__API_BASE__
    }
  } catch (e) {
    // ignore
  }
  // Vite injects VITE_* variables at build time
  if (import.meta && import.meta.env && import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL
  }
  // final fallback (matches backend mapping used in docker-compose)
  // Build-time override (Vite) if provided; otherwise same-origin
  if (import.meta && import.meta.env && import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL
  }
  // Same-origin default so the SPA calls its own host (no CORS)
  return ""
}

const api = axios.create({
  // Use full backend base (no trailing slash). Services call absolute paths like /api/...
  baseURL: getApiBase(),
})

api.interceptors.request.use((cfg) => {
  const t = localStorage.getItem("token")
  if (t) cfg.headers.Authorization = `Bearer ${t}`
  return cfg
})

export default api
