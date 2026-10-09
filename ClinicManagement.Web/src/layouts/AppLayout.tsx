import { useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import { Header } from '../components/Header'
import { Sidebar } from '../components/Sidebar'
import { navigationItems } from '../features/navigation/navigationItems'

export function AppLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const location = useLocation()
  const pageTitle =
    navigationItems.find((item) => item.path === location.pathname)?.label ?? 'Workspace'

  return (
    <div className="min-h-screen bg-[#f6f8f8] text-slate-900">
      <div className="flex min-h-screen">
        <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        <div className="min-w-0 flex-1">
          <Header onMenuClick={() => setSidebarOpen(true)} title={pageTitle} />
          <main className="mx-auto w-full max-w-[1440px] px-4 py-6 sm:px-7 sm:py-8 lg:px-10 lg:py-10">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}
