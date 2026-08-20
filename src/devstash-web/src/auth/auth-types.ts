export type AuthenticatedUser = {
  id: string
  displayName: string
  email: string
}

export type LoginInput = {
  email: string
  password: string
  rememberMe: boolean
}

export type RegisterInput = {
  displayName: string
  email: string
  password: string
  confirmPassword: string
}

export type FieldErrors = Record<string, string[]>

export class AuthApiError extends Error {
  readonly status: number | null
  readonly code: string
  readonly fieldErrors: FieldErrors

  constructor(
    message: string,
    options: {
      status?: number | null
      code?: string
      fieldErrors?: FieldErrors
    } = {},
  ) {
    super(message)
    this.name = 'AuthApiError'
    this.status = options.status ?? null
    this.code = options.code ?? 'unexpected_error'
    this.fieldErrors = options.fieldErrors ?? {}
  }
}
