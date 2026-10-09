import { UsersRound } from 'lucide-react'
import { ModulePlaceholder } from '../components/ModulePlaceholder'

export function PatientsPage() {
  return (
    <ModulePlaceholder
      description="The patient directory will show searchable, paginated patient records when API integration is added."
      icon={UsersRound}
      title="Patients"
    />
  )
}
