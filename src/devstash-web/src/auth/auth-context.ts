import { createContext } from 'react'
import type {
  AuthenticatedUser,
  LoginInput,
  RegisterInput,
} from './auth-types'

export type AuthStatus = 'loading' | 'ready' | 'error'

export type AuthContextValue = {
  user: AuthenticatedUser | null
  status: AuthStatus
  restoreSession: () => Promise<void>
  login: (input: LoginInput) => Promise<void>
  register: (input: RegisterInput) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
