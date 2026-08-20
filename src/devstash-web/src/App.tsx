import { Route, Routes } from 'react-router-dom'
import { AppShell } from './app/AppShell'
import { LoginPage } from './auth/LoginPage'
import { RegisterPage } from './auth/RegisterPage'
import {
  EntryRoute,
  ProtectedRoute,
  PublicOnlyRoute,
  SessionBoundary,
} from './auth/route-guards'
import './App.css'

export default function App() {
  return (
    <Routes>
      <Route element={<SessionBoundary />}>
        <Route element={<PublicOnlyRoute />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>
        <Route element={<ProtectedRoute />}>
          <Route path="/app" element={<AppShell />} />
        </Route>
        <Route path="*" element={<EntryRoute />} />
      </Route>
    </Routes>
  )
}
