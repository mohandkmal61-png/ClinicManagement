import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import { AppLayout } from './layouts/AppLayout'
import { AuthLayout } from './layouts/AuthLayout'
import { AppointmentsPage } from './pages/AppointmentsPage'
import { DoctorsPage } from './pages/DoctorsPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { MedicalRecordsPage } from './pages/MedicalRecordsPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { PatientsPage } from './pages/PatientsPage'
import { PrescriptionsPage } from './pages/PrescriptionsPage'
import { RegisterPage } from './pages/RegisterPage'

function ProtectedLayout() {
  const { session } = useAuth()
  const expiresAt = session ? Date.parse(session.expiresAtUtc) : Number.NaN

  if (!session || !Number.isFinite(expiresAt) || expiresAt <= Date.now()) {
    return <Navigate to="/login" replace />
  }

  return <AppLayout />
}

export default function App() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      <Route element={<ProtectedLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/patients" element={<PatientsPage />} />
        <Route path="/doctors" element={<DoctorsPage />} />
        <Route path="/appointments" element={<AppointmentsPage />} />
        <Route path="/medical-records" element={<MedicalRecordsPage />} />
        <Route path="/prescriptions" element={<PrescriptionsPage />} />
      </Route>

      <Route path="/404" element={<NotFoundPage />} />
      <Route path="*" element={<Navigate to="/404" replace />} />
    </Routes>
  )
}
