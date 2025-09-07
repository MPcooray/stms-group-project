// src/App.jsx
import { Routes, Route, Navigate } from 'react-router-dom'

function Home() { 
  return <div style={{ padding: 24, fontSize: 18 }}>Hello STMS 👋</div>
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
