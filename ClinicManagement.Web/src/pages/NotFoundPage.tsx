import { ArrowLeft, HeartPulse } from 'lucide-react'
import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-[#f6f8f8] px-5 py-10">
      <section className="surface-card w-full max-w-lg px-7 py-12 text-center sm:px-12">
        <span className="mx-auto flex size-14 items-center justify-center rounded-2xl bg-teal-50 text-teal-700">
          <HeartPulse aria-hidden="true" size={25} />
        </span>
        <p className="mt-7 text-xs font-bold uppercase tracking-[0.16em] text-teal-700">Page not found</p>
        <h1 className="mt-2 text-5xl font-bold tracking-tight text-slate-900">404</h1>
        <p className="mx-auto mt-3 max-w-sm text-sm leading-6 text-slate-500">
          We couldn’t find that page. It may have moved, or the address may be incorrect.
        </p>
        <Link
          className="mt-7 inline-flex items-center gap-2 rounded-xl bg-teal-700 px-4 py-2.5 text-xs font-bold text-white transition hover:bg-teal-800"
          to="/"
        >
          <ArrowLeft aria-hidden="true" size={15} /> Return to workspace
        </Link>
      </section>
    </main>
  )
}
