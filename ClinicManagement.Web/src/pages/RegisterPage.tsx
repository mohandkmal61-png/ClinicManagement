import { ArrowRight, UserRoundPlus } from 'lucide-react'
import { Link } from 'react-router-dom'

export function RegisterPage() {
  return (
    <div>
      <span className="flex size-12 items-center justify-center rounded-2xl bg-teal-50 text-teal-700">
        <UserRoundPlus aria-hidden="true" size={22} />
      </span>
      <p className="mt-8 text-xs font-bold uppercase tracking-[0.16em] text-teal-700">
        Join your team
      </p>
      <h1 className="mt-2 text-3xl font-bold tracking-tight text-slate-900">Create account</h1>
      <p className="mt-2 text-sm leading-6 text-slate-500">
        Registration will be connected to the clinic API in a following step.
      </p>
      <div className="mt-8 rounded-2xl border border-slate-200 bg-slate-50 p-5">
        <p className="text-sm font-semibold text-slate-800">Account creation is not connected yet</p>
        <p className="mt-2 text-xs leading-5 text-slate-500">
          This page is a visual route placeholder. No account data is collected or submitted.
        </p>
      </div>
      <p className="mt-7 text-center text-xs text-slate-500">
        Already registered?{' '}
        <Link className="font-semibold text-teal-700 hover:text-teal-800" to="/login">
          Sign in <ArrowRight aria-hidden="true" className="inline" size={13} />
        </Link>
      </p>
    </div>
  )
}
