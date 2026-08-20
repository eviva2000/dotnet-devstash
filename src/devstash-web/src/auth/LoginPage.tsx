import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { AuthApiError, type FieldErrors } from './auth-types'
import { useAuth } from './useAuth'
import { AuthLayout, ErrorSummary } from './AuthLayout'
import { firstError, focusFirstError, isValidEmail } from './form-utils'

type LoginLocationState = { registrationComplete?: boolean } | null

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const formRef = useRef<HTMLFormElement>(null)
  const [registrationComplete] = useState(
    Boolean((location.state as LoginLocationState)?.registrationComplete),
  )
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if ((location.state as LoginLocationState)?.registrationComplete) {
      void navigate('/login', { replace: true, state: null })
    }
  }, [location.state, navigate])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const trimmedEmail = email.trim()
    const errors: FieldErrors = {}

    if (trimmedEmail === '') {
      errors.email = ['Email is required.']
    } else if (!isValidEmail(trimmedEmail)) {
      errors.email = ['Enter a valid email address.']
    }
    if (password === '') {
      errors.password = ['Password is required.']
    }

    setFieldErrors(errors)
    setFormError('')
    if (Object.keys(errors).length > 0) {
      focusFirstError(formRef.current)
      return
    }

    setSubmitting(true)
    try {
      await login({ email: trimmedEmail, password, rememberMe })
      void navigate('/app', { replace: true })
    } catch (error) {
      setPassword('')
      if (error instanceof AuthApiError) {
        setFieldErrors(error.fieldErrors)
        setFormError(error.message)
      } else {
        setFormError('DevStash could not complete the request. Please try again.')
      }
      focusFirstError(formRef.current)
    } finally {
      setSubmitting(false)
    }
  }

  const emailError = firstError(fieldErrors, 'email')
  const passwordError = firstError(fieldErrors, 'password')

  return (
    <AuthLayout
      eyebrow="Welcome back"
      title="Sign in to DevStash"
      intro="Your snippets, prompts, commands, and notes are waiting."
      footer={
        <>
          New to DevStash? <Link to="/register">Create an account</Link>
        </>
      }
    >
      {registrationComplete && (
        <div className="success-message" role="status">
          Account created. Sign in with your new credentials.
        </div>
      )}
      <form ref={formRef} className="auth-form" onSubmit={handleSubmit} noValidate>
        {formError && <ErrorSummary message={formError} />}

        <div className="field">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            aria-invalid={emailError ? true : undefined}
            aria-describedby={emailError ? 'email-error' : undefined}
          />
          {emailError && (
            <p id="email-error" className="field-error">
              {emailError}
            </p>
          )}
        </div>

        <div className="field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            aria-invalid={passwordError ? true : undefined}
            aria-describedby={passwordError ? 'password-error' : undefined}
          />
          {passwordError && (
            <p id="password-error" className="field-error">
              {passwordError}
            </p>
          )}
        </div>

        <label className="checkbox-field">
          <input
            type="checkbox"
            checked={rememberMe}
            onChange={(event) => setRememberMe(event.target.checked)}
          />
          <span>Keep me signed in</span>
        </label>

        <button className="primary-button" type="submit" disabled={submitting}>
          {submitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </AuthLayout>
  )
}
