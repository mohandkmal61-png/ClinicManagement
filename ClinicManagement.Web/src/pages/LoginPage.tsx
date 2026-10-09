import { zodResolver } from '@hookform/resolvers/zod'
import axios from 'axios'
import { ArrowRight, Eye, EyeOff, KeyRound, LoaderCircle } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { useAuth } from '../context/AuthContext'
import { apiClient } from '../services/apiClient'
import type { AuthResponse } from '../types/auth'

const loginSchema = z.object({
  email: z.email('Enter a valid email address.'),
  password: z.string().min(1, 'Enter your password.'),
})

type LoginFormValues = z.infer<typeof loginSchema>

function getLoginErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return 'We could not sign you in. Please try again.'
  }

  switch (error.response?.status) {
    case 400:
      return 'Please check your email and password, then try again.'
    case 401:
      return 'The email or password is incorrect.'
    case 429:
      return 'Too many login attempts. Please wait a minute before trying again.'
    default:
      return error.response
        ? 'The clinic service is temporarily unavailable. Please try again.'
        : 'Unable to reach the clinic service. Check your connection and try again.'
  }
}

export function LoginPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [showPassword, setShowPassword] = useState(false)
  const [requestError, setRequestError] = useState<string | null>(null)
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  const onSubmit = handleSubmit(async ({ email, password }) => {
    setRequestError(null)

    try {
      const response = await apiClient.post<AuthResponse>('/api/Auth/login', {
        email,
        password,
      })
      signIn(response.data)
      navigate('/', { replace: true })
    } catch (error: unknown) {
      setRequestError(getLoginErrorMessage(error))
    }
  })

  return (
    <div>
      <span className="flex size-12 items-center justify-center rounded-2xl bg-teal-50 text-teal-700">
        <KeyRound aria-hidden="true" size={22} />
      </span>
      <p className="mt-8 text-xs font-bold uppercase tracking-[0.16em] text-teal-700">
        Welcome back
      </p>
      <h1 className="mt-2 text-3xl font-bold tracking-tight text-slate-900">Sign in</h1>
      <p className="mt-2 text-sm leading-6 text-slate-500">
        Sign in to continue to your clinic workspace.
      </p>

      <form className="mt-8 space-y-5" noValidate onSubmit={onSubmit}>
        {requestError && (
          <div
            aria-live="polite"
            className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
            role="alert"
          >
            {requestError}
          </div>
        )}

        <div>
          <label className="mb-2 block text-sm font-semibold text-slate-700" htmlFor="email">
            Email address
          </label>
          <input
            autoComplete="email"
            className="w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10"
            id="email"
            placeholder="you@clinic.com"
            type="email"
            aria-invalid={Boolean(errors.email)}
            aria-describedby={errors.email ? 'email-error' : undefined}
            {...register('email')}
          />
          {errors.email && (
            <p className="mt-1.5 text-xs text-rose-700" id="email-error">
              {errors.email.message}
            </p>
          )}
        </div>

        <div>
          <label className="mb-2 block text-sm font-semibold text-slate-700" htmlFor="password">
            Password
          </label>
          <div className="relative">
            <input
              autoComplete="current-password"
              className="w-full rounded-xl border border-slate-200 bg-white px-4 py-3 pr-12 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10"
              id="password"
              placeholder="Enter your password"
              type={showPassword ? 'text' : 'password'}
              aria-invalid={Boolean(errors.password)}
              aria-describedby={errors.password ? 'password-error' : undefined}
              {...register('password')}
            />
            <button
              aria-label={showPassword ? 'Hide password' : 'Show password'}
              className="absolute inset-y-0 right-0 flex items-center px-4 text-slate-400 transition hover:text-slate-600"
              onClick={() => setShowPassword((visible) => !visible)}
              type="button"
            >
              {showPassword ? <EyeOff aria-hidden="true" size={17} /> : <Eye aria-hidden="true" size={17} />}
            </button>
          </div>
          {errors.password && (
            <p className="mt-1.5 text-xs text-rose-700" id="password-error">
              {errors.password.message}
            </p>
          )}
        </div>

        <button
          className="flex w-full items-center justify-center gap-2 rounded-xl bg-teal-700 px-4 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-teal-800 focus:outline-none focus:ring-4 focus:ring-teal-700/20 disabled:cursor-not-allowed disabled:opacity-60"
          disabled={isSubmitting}
          type="submit"
        >
          {isSubmitting ? (
            <>
              <LoaderCircle aria-hidden="true" className="animate-spin" size={17} />
              Signing in…
            </>
          ) : (
            <>
              Sign in
              <ArrowRight aria-hidden="true" size={16} />
            </>
          )}
        </button>
      </form>

      <p className="mt-7 text-center text-xs text-slate-500">
        Need an account?{' '}
        <Link className="font-semibold text-teal-700 hover:text-teal-800" to="/register">
          Create one <ArrowRight aria-hidden="true" className="inline" size={13} />
        </Link>
      </p>
      <p className="mt-4 text-center text-[11px] leading-5 text-slate-400">
        Your sign-in ends when this page is reloaded. For now, your session is kept only in memory.
      </p>
    </div>
  )
}
