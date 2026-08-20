import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export function AppShell() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function handleLogout() {
    setSubmitting(true)
    setError('')
    try {
      await logout()
      void navigate('/login', { replace: true })
    } catch {
      setError('DevStash could not sign you out. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link className="app-brand" to="/app" aria-label="DevStash home">
          <span className="brand-mark small" aria-hidden="true">
            &lt;/&gt;
          </span>
          <span>DevStash</span>
        </Link>
        <button
          className="secondary-button"
          type="button"
          disabled={submitting}
          onClick={() => void handleLogout()}
        >
          {submitting ? 'Signing out…' : 'Sign out'}
        </button>
      </header>

      <main className="workspace">
        <p className="eyebrow">Authentication ready</p>
        <h1>Welcome, {user?.displayName}</h1>
        <p className="workspace-intro">
          Your secure DevStash workspace is ready for its first items.
        </p>

        {error && (
          <div className="error-summary shell-error" role="alert">
            <span aria-hidden="true">!</span>
            <p>{error}</p>
          </div>
        )}

        <section className="profile-card" aria-labelledby="profile-heading">
          <div>
            <p className="card-kicker">Signed in as</p>
            <h2 id="profile-heading">{user?.displayName}</h2>
            <p>{user?.email}</p>
          </div>
          <span className="secure-badge">Secure session</span>
        </section>
      </main>
    </div>
  )
}
