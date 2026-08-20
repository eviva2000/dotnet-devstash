import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

export function SessionBoundary() {
  const { status, restoreSession } = useAuth()

  if (status === 'loading') {
    return (
      <main className="status-page" aria-busy="true">
        <div className="spinner" aria-hidden="true" />
        <h1>Opening DevStash</h1>
        <p>Restoring your secure session…</p>
      </main>
    )
  }

  if (status === 'error') {
    return (
      <main className="status-page">
        <p className="eyebrow">Connection interrupted</p>
        <h1>We couldn’t open DevStash</h1>
        <p>Check that the API is running, then try again.</p>
        <button className="primary-button compact" onClick={() => void restoreSession()}>
          Try again
        </button>
      </main>
    )
  }

  return <Outlet />
}

export function PublicOnlyRoute() {
  const { user } = useAuth()
  return user ? <Navigate to="/app" replace /> : <Outlet />
}

export function ProtectedRoute() {
  const { user } = useAuth()
  return user ? <Outlet /> : <Navigate to="/login" replace />
}

export function EntryRoute() {
  const { user } = useAuth()
  return <Navigate to={user ? '/app' : '/login'} replace />
}
