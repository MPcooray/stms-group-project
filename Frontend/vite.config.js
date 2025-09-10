import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    open: true,
    proxy: {
      // proxy /api to backend when directly calling relative paths
      "/api": {
        target: process.env.VITE_API_BASE_URL || "http://localhost:5287",
        changeOrigin: true,
        secure: false,
      },
      "/auth": {
        target: process.env.VITE_API_BASE_URL || "http://localhost:5287",
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
