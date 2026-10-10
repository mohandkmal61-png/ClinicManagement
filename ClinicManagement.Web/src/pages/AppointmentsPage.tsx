import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { CalendarDays, LoaderCircle, Plus, Trash2 } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { apiClient } from '../services/apiClient'

interface Appointment {
  id: number
  doctorId: number
  patientId: number
  appointmentDate: string
  status: number
}

interface AppointmentForm {
  doctorId: string
  patientId: string
  appointmentDate: string
  status: number
}

const statusLabels = ['Scheduled', 'Completed', 'Cancelled', 'NoShow']
const emptyForm: AppointmentForm = { doctorId: '', patientId: '', appointmentDate: '', status: 0 }

function httpStatus(error: unknown): number | undefined {
  return axios.isAxiosError(error) ? error.response?.status : undefined
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function errorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const body: unknown = error.response?.data
    if (isRecord(body)) {
      if (isRecord(body.errors)) {
        const messages = Object.values(body.errors).flatMap((value) =>
          Array.isArray(value) ? value.filter((message): message is string => typeof message === 'string') : [],
        )
        if (messages.length) return messages.join(' ')
      }
      const record = body
      if (typeof record.message === 'string') return record.message
      if (typeof record.detail === 'string') return record.detail
      if (typeof record.title === 'string') return record.title
    }
    if (typeof body === 'string' && body.trim()) return body
  }
  return 'The request could not be completed. Please try again.'
}

function authorizationMessage(error: unknown): string | null {
  if (httpStatus(error) === 401) return 'Your session has expired or is invalid. Sign in again to continue.'
  if (httpStatus(error) === 403) return 'Your account does not have permission to access appointments.'
  return null
}

function localDateTimeValue(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return localDate.toISOString().slice(0, 19)
}

function displayDate(value: string): string {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date)
}

export function AppointmentsPage() {
  const { signOut } = useAuth()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Appointment | null>(null)
  const [form, setForm] = useState<AppointmentForm>(emptyForm)
  const [initialLocalDateTime, setInitialLocalDateTime] = useState('')
  const [formError, setFormError] = useState('')
  const [success, setSuccess] = useState('')

  const appointmentsQuery = useQuery({
    queryKey: ['appointments'],
    queryFn: async () => (await apiClient.get<Appointment[]>('/api/Appointments')).data,
  })

  const saveMutation = useMutation({
    mutationFn: async (input: { id: number | null; form: AppointmentForm; originalDate: string | null }) => {
      const parsedDoctorId = Number(input.form.doctorId)
      const parsedPatientId = Number(input.form.patientId)
      const dateWasUnchanged = input.id !== null && input.form.appointmentDate === initialLocalDateTime
      const appointmentDate = dateWasUnchanged && input.originalDate
        ? input.originalDate
        : new Date(input.form.appointmentDate).toISOString()
      const body = {
        doctorId: parsedDoctorId,
        patientId: parsedPatientId,
        appointmentDate,
        ...(input.id !== null ? { status: input.form.status } : {}),
      }

      if (input.id === null) return (await apiClient.post<Appointment>('/api/Appointments', body)).data
      await apiClient.put(`/api/Appointments/${input.id}`, body)
    },
    onSuccess: async () => {
      setSuccess(editing ? 'Appointment updated successfully.' : 'Appointment scheduled successfully.')
      setFormOpen(false)
      setEditing(null)
      setForm(emptyForm)
      await queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => apiClient.delete(`/api/Appointments/${id}`),
    onSuccess: async () => {
      setSuccess('Appointment deleted successfully.')
      await queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
  })

  function openCreateForm() {
    setSuccess('')
    setEditing(null)
    setForm(emptyForm)
    setInitialLocalDateTime('')
    saveMutation.reset()
    setFormOpen(true)
  }

  function openEditForm(appointment: Appointment) {
    const localValue = localDateTimeValue(appointment.appointmentDate)
    setSuccess('')
    setEditing(appointment)
    setInitialLocalDateTime(localValue)
    setForm({
      doctorId: String(appointment.doctorId),
      patientId: String(appointment.patientId),
      appointmentDate: localValue,
      status: appointment.status,
    })
    saveMutation.reset()
    setFormOpen(true)
  }

  function submitForm(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const doctorId = Number(form.doctorId)
    const patientId = Number(form.patientId)
    if (!Number.isInteger(doctorId) || doctorId < 1 || !Number.isInteger(patientId) || patientId < 1 || !form.appointmentDate) {
      setFormError('Enter valid doctor and patient IDs and select a date and time.')
      return
    }
    setFormError('')
    setSuccess('')
    saveMutation.mutate({
      id: editing?.id ?? null,
      form,
      originalDate: editing?.appointmentDate ?? null,
    })
  }

  const listAuthMessage = appointmentsQuery.isError ? authorizationMessage(appointmentsQuery.error) : null

  return (
    <div className="space-y-6">
      <section className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.14em] text-teal-700">
            <CalendarDays aria-hidden="true" size={15} /> Scheduling
          </div>
          <h2 className="mt-2 text-2xl font-bold tracking-tight text-slate-900">Appointments</h2>
          <p className="mt-1 text-sm text-slate-500">Schedule and manage patient appointments.</p>
        </div>
        <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-teal-700 px-4 py-3 text-sm font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-60" disabled={saveMutation.isPending} onClick={openCreateForm} type="button">
          <Plus aria-hidden="true" size={17} /> Schedule Appointment
        </button>
      </section>

      {success && <p aria-live="polite" className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800" role="status">{success}</p>}

      {formOpen && (
        <section className="surface-card p-5 sm:p-6">
          <h3 className="text-lg font-bold text-slate-900">{editing ? 'Update appointment' : 'Schedule appointment'}</h3>
          <p className="mt-1 text-sm text-slate-500">Enter the existing doctor and patient IDs. Appointment rules are validated by the service.</p>
          <form className="mt-5 space-y-4" noValidate onSubmit={submitForm}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="text-sm font-medium text-slate-700">
                Doctor ID
                <input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} min="1" onChange={(event) => setForm((current) => ({ ...current, doctorId: event.target.value }))} type="number" value={form.doctorId} />
              </label>
              <label className="text-sm font-medium text-slate-700">
                Patient ID
                <input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} min="1" onChange={(event) => setForm((current) => ({ ...current, patientId: event.target.value }))} type="number" value={form.patientId} />
              </label>
              <label className="text-sm font-medium text-slate-700">
                Appointment date and time
                <input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, appointmentDate: event.target.value }))} step="1" type="datetime-local" value={form.appointmentDate} />
              </label>
              {editing && (
                <label className="text-sm font-medium text-slate-700">
                  Status
                  <select className="mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, status: Number(event.target.value) }))} value={form.status}>
                    {statusLabels.map((label, index) => <option key={label} value={index}>{label}</option>)}
                  </select>
                </label>
              )}
            </div>
            {formError && <p className="text-sm text-rose-700" role="alert">{formError}</p>}
            {saveMutation.isError && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800" role="alert">
                {httpStatus(saveMutation.error) === 401 ? <span>Your session has expired. <Link className="font-semibold underline" onClick={signOut} to="/login">Sign in again</Link></span> : authorizationMessage(saveMutation.error) ?? errorMessage(saveMutation.error)}
              </div>
            )}
            <div className="flex justify-end gap-3">
              <button className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-700" disabled={saveMutation.isPending} onClick={() => setFormOpen(false)} type="button">Cancel</button>
              <button className="inline-flex items-center gap-2 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white disabled:opacity-60" disabled={saveMutation.isPending} type="submit">
                {saveMutation.isPending && <LoaderCircle aria-hidden="true" className="animate-spin" size={16} />}{saveMutation.isPending ? 'Saving…' : 'Save appointment'}
              </button>
            </div>
          </form>
        </section>
      )}

      {appointmentsQuery.isPending ? (
        <section aria-live="polite" className="surface-card flex min-h-64 items-center justify-center gap-3 text-sm text-slate-600"><LoaderCircle aria-hidden="true" className="animate-spin text-teal-700" size={24} /> Loading appointments…</section>
      ) : appointmentsQuery.isError ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center" role="alert">
          <h3 className="text-lg font-bold text-slate-900">Appointments could not be loaded</h3>
          <p className="mt-2 text-sm text-slate-500">{listAuthMessage ?? errorMessage(appointmentsQuery.error)}</p>
          {httpStatus(appointmentsQuery.error) === 401 ? <Link className="mt-4 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white" onClick={signOut} to="/login">Sign in again</Link> : <button className="mt-4 rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold" onClick={() => void appointmentsQuery.refetch()} type="button">Try again</button>}
        </section>
      ) : appointmentsQuery.data.length === 0 ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center">
          <CalendarDays aria-hidden="true" className="text-teal-700" size={28} />
          <h3 className="mt-4 text-lg font-bold text-slate-900">No appointments yet</h3>
          <p className="mt-2 text-sm text-slate-500">Schedule an appointment to see it here.</p>
        </section>
      ) : (
        <section className="surface-card overflow-hidden">
          <div className="border-b border-slate-100 px-5 py-4"><p className="text-sm font-bold text-slate-800">{appointmentsQuery.data.length} appointments</p></div>
          <div className="divide-y divide-slate-100">
            {appointmentsQuery.data.map((appointment) => (
              <article className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6" key={appointment.id}>
                <div>
                  <h3 className="font-semibold text-slate-800">{displayDate(appointment.appointmentDate)}</h3>
                  <p className="mt-1 text-sm text-slate-500">Doctor #{appointment.doctorId} · Patient #{appointment.patientId}</p>
                  <span className="mt-2 inline-flex rounded-full bg-teal-50 px-2.5 py-1 text-xs font-semibold text-teal-800">{statusLabels[appointment.status] ?? 'Unknown status'}</span>
                </div>
                <div className="flex gap-2">
                  <button className="rounded-lg border border-slate-200 px-3 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-50" onClick={() => openEditForm(appointment)} type="button">Edit / status</button>
                  <button className="inline-flex items-center gap-1.5 rounded-lg border border-rose-200 px-3 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-50 disabled:opacity-60" disabled={deleteMutation.isPending} onClick={() => { if (window.confirm('Delete this appointment?')) { setSuccess(''); deleteMutation.mutate(appointment.id) } }} type="button">
                    {deleteMutation.isPending ? <LoaderCircle aria-hidden="true" className="animate-spin" size={15} /> : <Trash2 aria-hidden="true" size={15} />} Delete
                  </button>
                </div>
              </article>
            ))}
          </div>
          {deleteMutation.isError && <p className="border-t border-rose-100 bg-rose-50 px-5 py-3 text-sm text-rose-800" role="alert">{authorizationMessage(deleteMutation.error) ?? errorMessage(deleteMutation.error)}{httpStatus(deleteMutation.error) === 401 && <Link className="ml-2 font-semibold underline" onClick={signOut} to="/login">Sign in again</Link>}</p>}
        </section>
      )}
    </div>
  )
}
