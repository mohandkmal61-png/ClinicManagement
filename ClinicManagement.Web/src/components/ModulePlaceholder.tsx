import type { LucideIcon } from 'lucide-react'
import { ArrowUpRight, Sparkles } from 'lucide-react'

interface ModulePlaceholderProps {
  description: string
  icon: LucideIcon
  title: string
}

export function ModulePlaceholder({
  description,
  icon: Icon,
  title,
}: ModulePlaceholderProps) {
  return (
    <section className="surface-card mx-auto flex min-h-[360px] max-w-4xl flex-col items-center justify-center px-6 py-12 text-center sm:px-12">
      <div className="flex size-16 items-center justify-center rounded-[22px] bg-teal-50 text-teal-700 ring-1 ring-teal-100">
        <Icon aria-hidden="true" size={27} strokeWidth={1.8} />
      </div>
      <div className="mt-6 inline-flex items-center gap-1.5 rounded-full bg-slate-100 px-3 py-1 text-[10px] font-bold uppercase tracking-[0.12em] text-slate-500">
        <Sparkles aria-hidden="true" size={12} />
        Foundation ready
      </div>
      <h2 className="mt-4 text-2xl font-bold tracking-tight text-slate-900">{title}</h2>
      <p className="mt-2 max-w-md text-sm leading-6 text-slate-500">{description}</p>
      <div className="mt-7 inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-xs font-semibold text-slate-600 shadow-sm">
        This module will be built in a following step
        <ArrowUpRight aria-hidden="true" size={14} className="text-teal-700" />
      </div>
    </section>
  )
}
