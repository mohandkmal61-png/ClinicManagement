import { ArrowRight, CalendarDays, FileHeart, ShieldCheck, UsersRound } from 'lucide-react'
import { Link } from 'react-router-dom'

const areas = [
  {
    description: 'Keep patient records in one organized place.',
    icon: UsersRound,
    label: 'Patient directory',
    path: '/patients',
  },
  {
    description: 'Coordinate clinic visits and appointment status.',
    icon: CalendarDays,
    label: 'Appointment schedule',
    path: '/appointments',
  },
  {
    description: 'Access care notes and treatment history.',
    icon: FileHeart,
    label: 'Clinical records',
    path: '/medical-records',
  },
]

export function HomePage() {
  return (
    <div className="space-y-8">
      <section className="relative overflow-hidden rounded-[28px] bg-gradient-to-br from-teal-800 via-teal-800 to-emerald-900 px-6 py-8 text-white shadow-xl shadow-teal-950/10 sm:px-10 sm:py-10 lg:px-12">
        <div className="absolute -right-16 -top-28 size-80 rounded-full border border-white/10" />
        <div className="absolute -right-2 -top-12 size-52 rounded-full border border-white/10" />
        <div className="relative max-w-2xl">
          <span className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-3 py-1.5 text-[11px] font-semibold text-teal-50">
            <ShieldCheck aria-hidden="true" size={14} />
            Your clinic workspace
          </span>
          <h2 className="mt-5 text-3xl font-bold tracking-tight sm:text-4xl">
            Good care starts with
            <br className="hidden sm:block" /> a connected team.
          </h2>
          <p className="mt-3 max-w-lg text-sm leading-6 text-teal-50/80">
            A clear place to navigate the tools that support your clinic. Choose a workspace area
            to get started.
          </p>
          <Link
            className="mt-6 inline-flex items-center gap-2 rounded-xl bg-white px-4 py-2.5 text-xs font-bold text-teal-900 shadow-sm transition hover:bg-teal-50"
            to="/appointments"
          >
            Open appointments <ArrowRight aria-hidden="true" size={15} />
          </Link>
        </div>
        <div className="absolute bottom-9 right-12 hidden size-32 items-center justify-center rounded-[30px] border border-white/15 bg-white/10 text-teal-100/90 lg:flex">
          <CalendarDays aria-hidden="true" size={58} strokeWidth={1.1} />
        </div>
      </section>

      <section>
        <div className="mb-4 flex items-end justify-between">
          <div>
            <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-teal-700">
              Workspace
            </p>
            <h2 className="mt-1 text-xl font-bold tracking-tight text-slate-900">Explore your clinic</h2>
          </div>
          <span className="hidden text-xs text-slate-400 sm:block">Your tools, all in one place</span>
        </div>
        <div className="grid gap-4 md:grid-cols-3">
          {areas.map(({ description, icon: Icon, label, path }) => (
            <Link
              className="surface-card group p-5 transition hover:-translate-y-0.5 hover:border-teal-200 hover:shadow-md"
              key={path}
              to={path}
            >
              <span className="flex size-11 items-center justify-center rounded-2xl bg-teal-50 text-teal-700 transition group-hover:bg-teal-100">
                <Icon aria-hidden="true" size={21} strokeWidth={1.8} />
              </span>
              <h3 className="mt-5 flex items-center justify-between text-sm font-bold text-slate-800">
                {label}
                <ArrowRight
                  aria-hidden="true"
                  className="text-slate-300 transition group-hover:translate-x-1 group-hover:text-teal-700"
                  size={16}
                />
              </h3>
              <p className="mt-1.5 text-xs leading-5 text-slate-500">{description}</p>
            </Link>
          ))}
        </div>
      </section>

      <section className="surface-card flex flex-col gap-4 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6">
        <div>
          <p className="text-sm font-semibold text-slate-800">Frontend foundation is ready</p>
          <p className="mt-1 text-xs text-slate-500">
            Data views and API connections will be added in the next implementation steps.
          </p>
        </div>
        <span className="w-fit rounded-full bg-amber-50 px-3 py-1.5 text-[10px] font-bold uppercase tracking-wide text-amber-700">
          Preview mode
        </span>
      </section>
    </div>
  )
}
