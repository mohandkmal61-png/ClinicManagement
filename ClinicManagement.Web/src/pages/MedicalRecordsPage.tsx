import { FileHeart } from 'lucide-react'
import { ModulePlaceholder } from '../components/ModulePlaceholder'

export function MedicalRecordsPage() {
  return (
    <ModulePlaceholder
      description="Medical record browsing and editing are not part of this initial frontend scaffold."
      icon={FileHeart}
      title="Medical records"
    />
  )
}
