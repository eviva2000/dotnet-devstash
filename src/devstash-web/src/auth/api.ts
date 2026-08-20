import {
  AuthApiError,
  type AuthenticatedUser,
  type FieldErrors,
  type LoginInput,
  type RegisterInput,
} from './auth-types'

type ProblemDetails = {
  title?: unknown
  detail?: unknown
  code?: unknown
  errors?: unknown
}

const genericErrorMessage =
  'DevStash could not complete the request. Please try again.'

async function parseJson(response: Response): Promise<unknown> {
  try {
    return await response.json()
  } catch {
    throw new AuthApiError(genericErrorMessage, {
      status: response.status,
      code: 'invalid_response',
    })
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function readFieldErrors(value: unknown): FieldErrors {
  if (!isRecord(value)) {
    return {}
  }

  return Object.fromEntries(
    Object.entries(value).flatMap(([field, messages]) => {
      if (!Array.isArray(messages)) {
        return []
      }

      const safeMessages = messages.filter(
        (message): message is string => typeof message === 'string',
      )
      return safeMessages.length > 0 ? [[field, safeMessages]] : []
    }),
  )
}

function apiError(response: Response, body: unknown): AuthApiError {
  const problem: ProblemDetails = isRecord(body) ? body : {}
  const code = typeof problem.code === 'string' ? problem.code : 'request_failed'
  const fieldErrors = readFieldErrors(problem.errors)

  let message = genericErrorMessage
  if (code === 'invalid_credentials') {
    message = 'The email or password is invalid.'
  } else if (code === 'invalid_csrf_token') {
    message = 'Your secure form session expired. Please try again.'
  } else if (Object.keys(fieldErrors).length > 0) {
    message = 'Please correct the highlighted fields.'
  }

  return new AuthApiError(message, {
    status: response.status,
    code,
    fieldErrors,
  })
}

async function requestJson<T>(
  path: string,
  init?: RequestInit,
): Promise<T> {
  let response: Response
  try {
    response = await fetch(path, {
      credentials: 'same-origin',
      ...init,
    })
  } catch {
    throw new AuthApiError(genericErrorMessage, { code: 'network_error' })
  }

  const body = await parseJson(response)
  if (!response.ok) {
    throw apiError(response, body)
  }

  return body as T
}

async function csrfToken(): Promise<string> {
  const response = await requestJson<{ requestToken?: unknown }>('/api/auth/csrf')
  if (typeof response.requestToken !== 'string' || response.requestToken === '') {
    throw new AuthApiError(genericErrorMessage, { code: 'invalid_response' })
  }

  return response.requestToken
}

async function authenticatedWrite<T>(
  path: string,
  body?: object,
): Promise<T | null> {
  const token = await csrfToken()
  let response: Response

  try {
    response = await fetch(path, {
      method: 'POST',
      credentials: 'same-origin',
      headers: {
        'Content-Type': 'application/json',
        'X-XSRF-TOKEN': token,
      },
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new AuthApiError(genericErrorMessage, { code: 'network_error' })
  }

  if (response.status === 202 || response.status === 204) {
    return null
  }

  const responseBody = await parseJson(response)
  if (!response.ok) {
    throw apiError(response, responseBody)
  }

  return responseBody as T
}

export async function getCurrentUser(): Promise<AuthenticatedUser | null> {
  let response: Response
  try {
    response = await fetch('/api/auth/me', { credentials: 'same-origin' })
  } catch {
    throw new AuthApiError(genericErrorMessage, { code: 'network_error' })
  }

  if (response.status === 401) {
    return null
  }

  const body = await parseJson(response)
  if (!response.ok) {
    throw apiError(response, body)
  }

  return body as AuthenticatedUser
}

export async function login(input: LoginInput): Promise<AuthenticatedUser> {
  const user = await authenticatedWrite<AuthenticatedUser>('/api/auth/login', input)
  if (user === null) {
    throw new AuthApiError(genericErrorMessage, { code: 'invalid_response' })
  }

  return user
}

export async function register(input: RegisterInput): Promise<void> {
  await authenticatedWrite('/api/auth/register', input)
}

export async function logout(): Promise<void> {
  try {
    await authenticatedWrite('/api/auth/logout')
  } catch (error) {
    if (error instanceof AuthApiError && error.status === 401) {
      return
    }

    throw error
  }
}
