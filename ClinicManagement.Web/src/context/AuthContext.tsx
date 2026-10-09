import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { clearAccessToken, setAccessToken } from '../services/apiClient'
import type { AuthResponse } from '../types/auth'

interface AuthSession {
  email: string
  expiresAtUtc: string
}

interface AuthContextValue {
  session: AuthSession | null
  signIn: (response: AuthResponse) => void
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [session, setSession] = useState<AuthSession | null>(null)

  const signOut = useCallback(() => {
    clearAccessToken()
    setSession(null)
  }, [])

  const signIn = useCallback((response: AuthResponse) => {
    setAccessToken(response.accessToken)
    setSession({ email: response.email, expiresAtUtc: response.expiresAtUtc })
  }, [])

  useEffect(() => {
    if (!session) {
      return
    }

    const expiresAt = Date.parse(session.expiresAtUtc)
    if (!Number.isFinite(expiresAt)) {
      signOut()
      return
    }

    let timeoutId: number
    const expireSession = () => {
      const remainingTime = expiresAt - Date.now()
      if (remainingTime <= 0) {
        signOut()
        return
      }

      timeoutId = window.setTimeout(expireSession, Math.min(remainingTime, 2_147_483_647))
    }

    expireSession()
    return () => window.clearTimeout(timeoutId)
  }, [session, signOut])

  const value = useMemo<AuthContextValue>(
    () => ({ session, signIn, signOut }),
    [session, signIn, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.')
  }

  return context
}
