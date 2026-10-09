import { Activity, ArrowUpRight, HeartPulse, X } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { navigationItems } from '../features/navigation/navigationItems'

interface SidebarProps {
  isOpen: boolean
  onClose: () => void
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  return (
    <>
      {isOpen && (
        <button
          aria-label="Close navigation"
          className="fixed inset-0 z-30 bg-slate-950/35 backdrop-blur-[2px] lg:hidden"
          onClick={onClose}
          type="button"
        />
      )}
      <aside
        className={`fixed inset-y-0 left-0 z-40 flex w-[270px] flex-col border-r border-slate-200/80 bg-white px-5 pb-5 pt-6 transition-transform duration-300 lg:static lg:z-auto lg:translate-x-0 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex items-center justify-between px-1">
          <NavLink className="flex items-center gap-3" onClick={onClose} to="/">
            <span className="flex size-11 items-center justify-center rounded-2xl bg-teal-700 text-white shadow-lg shadow-teal-900/15">
              <HeartPulse aria-hidden="true" size={22} strokeWidth={2.2} />
            </span>
            <span>
              <span className="block text-[15px] font-bold tracking-tight text-slate-900">
                ClinicCare
              </span>
              <span className="mt-0.5 block text-[11px] font-medium tracking-wide text-slate-400">
                MANAGEMENT
              </span>
            </span>
          </NavLink>
          <button
            aria-label="Close navigation"
            className="rounded-xl p-2 text-slate-500 hover:bg-slate-100 lg:hidden"
            onClick={onClose}
            type="button"
          >
            <X size={19} />
          </button>
        </div>

        <div className="mt-10 px-3 text-[10px] font-bold uppercase tracking-[0.16em] text-slate-400">
          Workspace
        </div>
        <nav aria-label="Main navigation" className="mt-3 space-y-1">
          {navigationItems.map(({ icon: Icon, label, path }) => (
            <NavLink
              key={path}
              className={({ isActive }) =>
                `group flex items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-semibold transition ${
                  isActive
                    ? 'bg-teal-50 text-teal-800'
                    : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'
                }`
              }
              end={path === '/'}
              onClick={onClose}
              to={path}
            >
              {({ isActive }) => (
                <>
                  <Icon
                    aria-hidden="true"
                    className={isActive ? 'text-teal-700' : 'text-slate-400 group-hover:text-slate-600'}
                    size={18}
                    strokeWidth={1.9}
                  />
                  <span className="flex-1">{label}</span>
                  {isActive && <span className="size-1.5 rounded-full bg-teal-600" />}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="mt-auto">
          <div className="overflow-hidden rounded-2xl bg-slate-900 p-4 text-white">
            <div className="flex size-9 items-center justify-center rounded-xl bg-white/10 text-teal-200">
              <Activity aria-hidden="true" size={18} />
            </div>
            <p className="mt-4 text-sm font-semibold">A healthier workflow</p>
            <p className="mt-1.5 text-xs leading-5 text-slate-300">
              Your clinic tools, thoughtfully brought together.
            </p>
            <div className="mt-4 flex items-center gap-1 text-[11px] font-semibold text-teal-200">
              Care, connected <ArrowUpRight aria-hidden="true" size={14} />
            </div>
          </div>
          <p className="mt-4 px-2 text-[10px] text-slate-400">ClinicCare workspace · UI preview</p>
        </div>
      </aside>
    </>
  )
}
