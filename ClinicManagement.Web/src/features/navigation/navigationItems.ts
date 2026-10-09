import type { LucideIcon } from 'lucide-react'
import {
  CalendarDays,
  FileHeart,
  LayoutDashboard,
  Pill,
  Stethoscope,
  UsersRound,
} from 'lucide-react'

export interface NavigationItem {
  label: string
  path: string
  icon: LucideIcon
}

export const navigationItems: NavigationItem[] = [
  { label: 'Overview', path: '/', icon: LayoutDashboard },
  { label: 'Patients', path: '/patients', icon: UsersRound },
  { label: 'Doctors', path: '/doctors', icon: Stethoscope },
  { label: 'Appointments', path: '/appointments', icon: CalendarDays },
  { label: 'Medical records', path: '/medical-records', icon: FileHeart },
  { label: 'Prescriptions', path: '/prescriptions', icon: Pill },
]
