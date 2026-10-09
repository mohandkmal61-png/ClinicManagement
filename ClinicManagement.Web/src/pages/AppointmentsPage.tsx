import { CalendarDays } from 'lucide-react'
import { ModulePlaceholder } from '../components/ModulePlaceholder'

export function AppointmentsPage() {
  return (
    <ModulePlaceholder
      description="Appointment scheduling, status updates, and conflict feedback will be connected to the API later."
      icon={CalendarDays}
      title="Appointments"
    />
  )
}
