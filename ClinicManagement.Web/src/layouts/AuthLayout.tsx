import { Activity, HeartPulse, ShieldCheck } from 'lucide-react'
import { Link, Outlet } from 'react-router-dom'

export function AuthLayout() {
  return (
    <main className="grid min-h-screen bg-white lg:grid-cols-[1fr_0.92fr]">
      <section className="relative hidden overflow-hidden bg-teal-950 px-12 py-10 text-white lg:flex lg:flex-col xl:px-20">
        <div className="absolute -right-28 top-24 size-[440px] rounded-full border border-teal-700/30" />
        <div className="absolute -right-12 top-40 size-[310px] rounded-full border border-teal-700/30" />
        <div className="relative flex items-center gap-3">
          <span className="flex size-11 items-center justify-center rounded-2xl bg-teal-500 text-white">
            <HeartPulse aria-hidden="true" size={23} />
          </span>
          <span className="text-sm font-bold tracking-wide">ClinicCare</span>
        </div>

        <div className="relative my-auto max-w-xl py-16">
          <div className="mb-7 inline-flex items-center gap-2 rounded-full border border-teal-700/70 bg-teal-900/50 px-3 py-1.5 text-xs font-medium text-teal-100">
            <Activity aria-hidden="true" size={14} />
            A calmer way to care
          </div>
          <h1 className="text-5xl font-semibold leading-[1.12] tracking-tight xl:text-6xl">
            Care teams,
            <br />
            <span className="text-teal-300">connected.</span>
          </h1>
          <p className="mt-6 max-w-md text-base leading-7 text-teal-100/75">
            A thoughtful workspace for the people who make a difference in every patient’s day.
          </p>
          <div className="mt-12 flex items-center gap-3 text-xs text-teal-100/70">
            <ShieldCheck aria-hidden="true" size={17} />
            Secure clinic workspace
          </div>
        </div>
        <p className="relative text-xs text-teal-100/50">ClinicCare · Clinic Management</p>
      </section>

      <section className="flex min-h-screen flex-col px-5 py-6 sm:px-10 lg:px-14 xl:px-20">
        <Link className="flex items-center gap-2.5 text-sm font-bold text-slate-800 lg:hidden" to="/">
          <span className="flex size-9 items-center justify-center rounded-xl bg-teal-700 text-white">
            <HeartPulse aria-hidden="true" size={19} />
          </span>
          ClinicCare
        </Link>
        <div className="flex flex-1 items-center justify-center py-10">
          <div className="w-full max-w-md">
            <Outlet />
          </div>
        </div>
        <p className="text-center text-[11px] text-slate-400">
          © {new Date().getFullYear()} ClinicCare workspace
        </p>
      </section>
    </main>
  )
}
