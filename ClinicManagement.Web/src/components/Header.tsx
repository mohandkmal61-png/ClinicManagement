import { Bell, Menu, Search } from 'lucide-react'

interface HeaderProps {
  onMenuClick: () => void
  title: string
}

export function Header({ onMenuClick, title }: HeaderProps) {
  return (
    <header className="sticky top-0 z-20 flex h-[76px] items-center justify-between border-b border-slate-200/80 bg-white/90 px-4 backdrop-blur-xl sm:px-7 lg:px-10">
      <div className="flex min-w-0 items-center gap-3">
        <button
          aria-label="Open navigation"
          className="rounded-xl p-2 text-slate-600 transition hover:bg-slate-100 lg:hidden"
          onClick={onMenuClick}
          type="button"
        >
          <Menu size={20} />
        </button>
        <div className="min-w-0">
          <p className="hidden text-[10px] font-semibold uppercase tracking-[0.16em] text-slate-400 sm:block">
            Clinic workspace
          </p>
          <h1 className="truncate text-lg font-bold tracking-tight text-slate-900 sm:mt-0.5 sm:text-xl">
            {title}
          </h1>
        </div>
      </div>

      <div className="flex items-center gap-2 sm:gap-3">
        <div className="hidden h-10 w-60 items-center gap-2.5 rounded-xl border border-slate-200 bg-slate-50/70 px-3 text-slate-400 md:flex">
          <Search aria-hidden="true" size={16} />
          <span className="text-xs">Search workspace</span>
          <span className="ml-auto rounded-md border border-slate-200 bg-white px-1.5 py-0.5 text-[10px] font-medium text-slate-400">
            /
          </span>
        </div>
        <button
          aria-label="Notifications (not connected)"
          className="relative rounded-xl border border-slate-200 p-2.5 text-slate-500 transition hover:bg-slate-50"
          type="button"
        >
          <Bell aria-hidden="true" size={17} />
          <span className="absolute right-2 top-2 size-1.5 rounded-full bg-teal-500 ring-2 ring-white" />
        </button>
        <div
          aria-label="User profile placeholder"
          className="flex size-9 items-center justify-center rounded-full bg-amber-100 text-xs font-bold text-amber-800 ring-2 ring-white"
        >
          CC
        </div>
      </div>
    </header>
  )
}
