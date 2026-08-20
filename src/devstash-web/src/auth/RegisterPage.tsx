import { useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { AuthApiError, type FieldErrors } from './auth-types'
import { useAuth } from './useAuth'
import { AuthLayout, ErrorSummary } from './AuthLayout'
import { firstError, focusFirstError, isValidEmail } from './form-utils'

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const formRef = useRef<HTMLFormElement>(null)
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const trimmedDisplayName = displayName.trim()
    const trimmedEmail = email.trim()
    const errors: FieldErrors = {}

    if (trimmedDisplayName === '') {
      errors.displayName = ['Display name is required.']
    } else if (trimmedDisplayName.length > 100) {
      errors.displayName = ['Display name must be 100 characters or fewer.']
    }
    if (trimmedEmail === '') {
      errors.email = ['Email is required.']
    } else if (!isValidEmail(trimmedEmail)) {
      errors.email = ['Enter a valid email address.']
    }
    if (password === '') {
      errors.password = ['Password is required.']
    } else if (password.length < 8) {
      errors.password = ['Password must be at least 8 characters.']
    }
    if (confirmPassword !== password) {
      errors.confirmPassword = ['Password confirmation must match the password.']
    }

    setFieldErrors(errors)
    setFormError('')
    if (Object.keys(errors).length > 0) {
      focusFirstError(formRef.current)
      return
    }

    setSubmitting(true)
    try {
      await register({
        displayName: trimmedDisplayName,
        email: trimmedEmail,
        password,
        confirmPassword,
      })
      void navigate('/login', {
        replace: true,
        state: { registrationComplete: true },
      })
    } catch (error) {
      setPassword('')
      setConfirmPassword('')
      if (error instanceof AuthApiError) {
        setFieldErrors({ ...error.fieldErrors })
        setFormError(error.message)
      } else {
        setFormError('DevStash could not complete the request. Please try again.')
      }
      focusFirstError(formRef.current)
    } finally {
      setSubmitting(false)
    }
  }

  const displayNameError = firstError(fieldErrors, 'displayName')
  const emailError = firstError(fieldErrors, 'email')
  const passwordError = firstError(fieldErrors, 'password')
  const confirmPasswordError = firstError(fieldErrors, 'confirmPassword')

  return (
    <AuthLayout
      eyebrow="Start your stash"
      title="Create your account"
      intro="Build one searchable home for the developer knowledge you reuse."
      footer={
        <>
          Already have an account? <Link to="/login">Sign in</Link>
        </>
      }
    >
      <form ref={formRef} className="auth-form" onSubmit={handleSubmit} noValidate>
        {formError && <ErrorSummary message={formError} />}

        <div className="field">
          <label htmlFor="display-name">Display name</label>
          <input
            id="display-name"
            name="displayName"
            type="text"
            autoComplete="name"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            aria-invalid={displayNameError ? true : undefined}
            aria-describedby={displayNameError ? 'display-name-error' : undefined}
          />
          {displayNameError && (
            <p id="display-name-error" className="field-error">
              {displayNameError}
            </p>
          )}
        </div>

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
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            aria-invalid={passwordError ? true : undefined}
            aria-describedby={passwordError ? 'password-error' : 'password-hint'}
          />
          <p id="password-hint" className="field-hint">
            At least 8 characters
          </p>
          {passwordError && (
            <p id="password-error" className="field-error">
              {passwordError}
            </p>
          )}
        </div>

        <div className="field">
          <label htmlFor="confirm-password">Confirm password</label>
          <input
            id="confirm-password"
            name="confirmPassword"
            type="password"
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            aria-invalid={confirmPasswordError ? true : undefined}
            aria-describedby={
              confirmPasswordError ? 'confirm-password-error' : undefined
            }
          />
          {confirmPasswordError && (
            <p id="confirm-password-error" className="field-error">
              {confirmPasswordError}
            </p>
          )}
        </div>

        <button className="primary-button" type="submit" disabled={submitting}>
          {submitting ? 'Creating account…' : 'Create account'}
        </button>
      </form>
    </AuthLayout>
  )
}
