import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderApp } from '../test/test-utils'

const ada = {
  id: '08d6c6a6-cf75-4df2-adab-c61c38b20ec2',
  displayName: 'Ada Lovelace',
  email: 'ada@example.com',
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

function noContentResponse(): Response {
  return new Response(null, { status: 204 })
}

function fetchMock() {
  const mock = vi.fn<typeof fetch>()
  vi.stubGlobal('fetch', mock)
  return mock
}

afterEach(() => {
  vi.unstubAllGlobals()
  vi.restoreAllMocks()
})

describe('session and route behavior', () => {
  it('restores a session and renders the protected shell', async () => {
    const fetch = fetchMock().mockResolvedValue(jsonResponse(ada))

    renderApp('/app')

    expect(await screen.findByRole('heading', { name: 'Welcome, Ada Lovelace' })).toBeVisible()
    expect(screen.getByText('ada@example.com')).toBeVisible()
    expect(fetch).toHaveBeenCalledTimes(1)
    expect(fetch).toHaveBeenCalledWith('/api/auth/me', {
      credentials: 'same-origin',
    })
  })

  it('treats an unauthorized session as anonymous', async () => {
    fetchMock().mockResolvedValue(new Response(null, { status: 401 }))

    renderApp('/app')

    expect(await screen.findByRole('heading', { name: 'Sign in to DevStash' })).toBeVisible()
    expect(screen.queryByText(/Welcome,/)).not.toBeInTheDocument()
  })

  it('does not show protected content while session restoration is pending', () => {
    fetchMock().mockReturnValue(new Promise(() => {}))

    renderApp('/app')

    expect(screen.getByRole('heading', { name: 'Opening DevStash' })).toBeVisible()
    expect(screen.queryByText(/Welcome,/)).not.toBeInTheDocument()
  })

  it.each(['/login', '/register'])(
    'redirects an authenticated visitor away from %s',
    async (path) => {
      fetchMock().mockResolvedValue(jsonResponse(ada))

      renderApp(path)

      expect(
        await screen.findByRole('heading', { name: 'Welcome, Ada Lovelace' }),
      ).toBeVisible()
      expect(
        screen.queryByRole('heading', { name: 'Sign in to DevStash' }),
      ).not.toBeInTheDocument()
      expect(
        screen.queryByRole('heading', { name: 'Create your account' }),
      ).not.toBeInTheDocument()
    },
  )

  it('shows a retry state when session restoration fails', async () => {
    const fetch = fetchMock()
      .mockRejectedValueOnce(new TypeError('offline'))
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
    const user = userEvent.setup()

    renderApp('/login')

    expect(await screen.findByRole('heading', { name: 'We couldn’t open DevStash' })).toBeVisible()
    await user.click(screen.getByRole('button', { name: 'Try again' }))

    expect(await screen.findByRole('heading', { name: 'Sign in to DevStash' })).toBeVisible()
    expect(fetch).toHaveBeenCalledTimes(2)
  })
})

describe('authentication forms', () => {
  it('logs in with a fresh CSRF token and enters the protected shell', async () => {
    const fetch = fetchMock()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(jsonResponse({ requestToken: 'csrf-login' }))
      .mockResolvedValueOnce(jsonResponse(ada))
    const user = userEvent.setup()

    renderApp('/login')
    await screen.findByRole('heading', { name: 'Sign in to DevStash' })
    await user.type(screen.getByLabelText('Email'), ' ada@example.com ')
    await user.type(screen.getByLabelText('Password'), 'example-password')
    await user.click(screen.getByLabelText('Keep me signed in'))
    await user.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByRole('heading', { name: 'Welcome, Ada Lovelace' })).toBeVisible()
    expect(fetch).toHaveBeenNthCalledWith(
      3,
      '/api/auth/login',
      expect.objectContaining({
        method: 'POST',
        credentials: 'same-origin',
        headers: expect.objectContaining({ 'X-XSRF-TOKEN': 'csrf-login' }),
        body: JSON.stringify({
          email: 'ada@example.com',
          password: 'example-password',
          rememberMe: true,
        }),
      }),
    )
  })

  it('shows a generic invalid-credentials error', async () => {
    fetchMock()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(jsonResponse({ requestToken: 'csrf-login' }))
      .mockResolvedValueOnce(
        jsonResponse(
          {
            title: 'Invalid credentials',
            detail: 'The email or password is invalid.',
            code: 'invalid_credentials',
          },
          401,
        ),
      )
    const user = userEvent.setup()

    renderApp('/login')
    await screen.findByRole('heading', { name: 'Sign in to DevStash' })
    await user.type(screen.getByLabelText('Email'), 'ada@example.com')
    await user.type(screen.getByLabelText('Password'), 'wrong-password')
    await user.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByText('The email or password is invalid.')).toBeVisible()
    expect(screen.queryByText(/account does not exist/i)).not.toBeInTheDocument()
  })

  it('registers trimmed values and returns to login without authenticating', async () => {
    const fetch = fetchMock()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(jsonResponse({ requestToken: 'csrf-register' }))
      .mockResolvedValueOnce(jsonResponse(ada, 201))
    const user = userEvent.setup()

    renderApp('/register')
    await screen.findByRole('heading', { name: 'Create your account' })
    await user.type(screen.getByLabelText('Display name'), '  Ada Lovelace  ')
    await user.type(screen.getByLabelText('Email'), '  ada@example.com  ')
    await user.type(screen.getByLabelText('Password'), 'example-password')
    await user.type(screen.getByLabelText('Confirm password'), 'example-password')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(await screen.findByText('Account created. Sign in with your new credentials.')).toBeVisible()
    expect(screen.getByRole('heading', { name: 'Sign in to DevStash' })).toBeVisible()
    expect(fetch).toHaveBeenNthCalledWith(
      3,
      '/api/auth/register',
      expect.objectContaining({
        headers: expect.objectContaining({ 'X-XSRF-TOKEN': 'csrf-register' }),
        body: JSON.stringify({
          displayName: 'Ada Lovelace',
          email: 'ada@example.com',
          password: 'example-password',
          confirmPassword: 'example-password',
        }),
      }),
    )
  })

  it('associates a duplicate-email error with the registration field', async () => {
    fetchMock()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(jsonResponse({ requestToken: 'csrf-register' }))
      .mockResolvedValueOnce(
        jsonResponse(
          {
            title: 'Email already registered',
            code: 'email_already_registered',
          },
          409,
        ),
      )
    const user = userEvent.setup()

    renderApp('/register')
    await screen.findByRole('heading', { name: 'Create your account' })
    await user.type(screen.getByLabelText('Display name'), 'Ada Lovelace')
    await user.type(screen.getByLabelText('Email'), 'ada@example.com')
    await user.type(screen.getByLabelText('Password'), 'example-password')
    await user.type(screen.getByLabelText('Confirm password'), 'example-password')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    const email = screen.getByLabelText('Email')
    await waitFor(() => expect(email).toHaveAttribute('aria-invalid', 'true'))
    expect(screen.getAllByText('An account is already registered with this email address.')).toHaveLength(2)
  })

  it('validates registration before making a write request', async () => {
    const fetch = fetchMock().mockResolvedValueOnce(new Response(null, { status: 401 }))
    const user = userEvent.setup()

    renderApp('/register')
    await screen.findByRole('heading', { name: 'Create your account' })
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(screen.getByText('Display name is required.')).toBeVisible()
    expect(screen.getByText('Email is required.')).toBeVisible()
    expect(fetch).toHaveBeenCalledTimes(1)
  })

  it('logs out with a fresh CSRF token and returns to login', async () => {
    const fetch = fetchMock()
      .mockResolvedValueOnce(jsonResponse(ada))
      .mockResolvedValueOnce(jsonResponse({ requestToken: 'csrf-logout' }))
      .mockResolvedValueOnce(noContentResponse())
    const user = userEvent.setup()

    renderApp('/app')
    await screen.findByRole('heading', { name: 'Welcome, Ada Lovelace' })
    await user.click(screen.getByRole('button', { name: 'Sign out' }))

    expect(await screen.findByRole('heading', { name: 'Sign in to DevStash' })).toBeVisible()
    expect(fetch).toHaveBeenNthCalledWith(
      3,
      '/api/auth/logout',
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({ 'X-XSRF-TOKEN': 'csrf-logout' }),
      }),
    )
  })
})
