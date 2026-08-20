import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import * as authApi from './api'
import type {
  AuthenticatedUser,
  LoginInput,
  RegisterInput,
} from './auth-types'
import { AuthContext, type AuthStatus } from './auth-context'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthenticatedUser | null>(null)
  const [status, setStatus] = useState<AuthStatus>('loading')
  const initialRequest = useRef<Promise<AuthenticatedUser | null> | null>(null)
  const restorationId = useRef(0)

  const applyRestoration = useCallback(
    async (request: Promise<AuthenticatedUser | null>, id: number) => {
      try {
        const currentUser = await request
        if (restorationId.current === id) {
          setUser(currentUser)
          setStatus('ready')
        }
      } catch {
        if (restorationId.current === id) {
          setStatus('error')
        }
      }
    },
    [],
  )

  const restoreSession = useCallback(async () => {
    const id = ++restorationId.current
    setStatus('loading')
    const request = authApi.getCurrentUser()
    initialRequest.current = request
    await applyRestoration(request, id)
  }, [applyRestoration])

  useEffect(() => {
    const id = ++restorationId.current
    const request = initialRequest.current ?? authApi.getCurrentUser()
    initialRequest.current = request
    void applyRestoration(request, id)
  }, [applyRestoration])

  const login = useCallback(async (input: LoginInput) => {
    const currentUser = await authApi.login(input)
    setUser(currentUser)
    setStatus('ready')
  }, [])

  const register = useCallback(async (input: RegisterInput) => {
    await authApi.register(input)
  }, [])

  const logout = useCallback(async () => {
    await authApi.logout()
    setUser(null)
    setStatus('ready')
  }, [])

  const value = useMemo(
    () => ({ user, status, restoreSession, login, register, logout }),
    [user, status, restoreSession, login, register, logout],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
