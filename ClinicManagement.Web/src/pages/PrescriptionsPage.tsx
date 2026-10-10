import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { LoaderCircle, Pill, Plus, Trash2 } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { apiClient } from '../services/apiClient'

interface Prescription {
  id: number
  patientId: number
  doctorId: number
  medicationName: string
  dosage: string
  instructions: string
  createdAt: string
}

interface PrescriptionForm {
  patientId: string
  doctorId: string
  medicationName: string
  dosage: string
  instructions: string
}

const emptyForm: PrescriptionForm = {
  patientId: '',
  doctorId: '',
  medicationName: '',
  dosage: '',
  instructions: '',
}

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
  if (httpStatus(error) === 403) return 'Your account does not have permission to access prescriptions.'
  return null
}

function formatDate(value: string): string {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date)
}

export function PrescriptionsPage() {
  const { signOut } = useAuth()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Prescription | null>(null)
  const [form, setForm] = useState<PrescriptionForm>(emptyForm)
  const [formError, setFormError] = useState('')
  const [success, setSuccess] = useState('')

  const prescriptionsQuery = useQuery({
    queryKey: ['prescriptions'],
    queryFn: async () => (await apiClient.get<Prescription[]>('/api/Prescriptions')).data,
  })

  const saveMutation = useMutation({
    mutationFn: async (input: { id: number | null; values: PrescriptionForm }) => {
      const body = {
        patientId: Number(input.values.patientId),
        doctorId: Number(input.values.doctorId),
        medicationName: input.values.medicationName.trim(),
        dosage: input.values.dosage.trim(),
        instructions: input.values.instructions.trim(),
      }
      if (input.id === null) return (await apiClient.post<Prescription>('/api/Prescriptions', body)).data
      await apiClient.put(`/api/Prescriptions/${input.id}`, body)
    },
    onSuccess: async () => {
      setSuccess(editing ? 'Prescription updated successfully.' : 'Prescription added successfully.')
      setFormOpen(false)
      setEditing(null)
      setForm(emptyForm)
      await queryClient.invalidateQueries({ queryKey: ['prescriptions'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => apiClient.delete(`/api/Prescriptions/${id}`),
    onSuccess: async () => {
      setSuccess('Prescription deleted successfully.')
      await queryClient.invalidateQueries({ queryKey: ['prescriptions'] })
    },
  })

  function openCreateForm() {
    setSuccess('')
    setEditing(null)
    setForm(emptyForm)
    setFormError('')
    saveMutation.reset()
    setFormOpen(true)
  }

  function openEditForm(prescription: Prescription) {
    setSuccess('')
    setEditing(prescription)
    setForm({
      patientId: String(prescription.patientId),
      doctorId: String(prescription.doctorId),
      medicationName: prescription.medicationName,
      dosage: prescription.dosage,
      instructions: prescription.instructions,
    })
    setFormError('')
    saveMutation.reset()
    setFormOpen(true)
  }

  function submitForm(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const patientId = Number(form.patientId)
    const doctorId = Number(form.doctorId)
    if (!Number.isInteger(patientId) || patientId < 1 || !Number.isInteger(doctorId) || doctorId < 1 ||
        !form.medicationName.trim() || !form.dosage.trim() || !form.instructions.trim()) {
      setFormError('Enter valid patient and doctor IDs, medication, dosage, and instructions.')
      return
    }
    setFormError('')
    setSuccess('')
    saveMutation.mutate({ id: editing?.id ?? null, values: form })
  }

  const listAuthMessage = prescriptionsQuery.isError ? authorizationMessage(prescriptionsQuery.error) : null

  return (
    <div className="space-y-6">
      <section className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.14em] text-teal-700"><Pill aria-hidden="true" size={15} /> Medication</div>
          <h2 className="mt-2 text-2xl font-bold tracking-tight text-slate-900">Prescriptions</h2>
          <p className="mt-1 text-sm text-slate-500">Manage medication and instructions prescribed to patients.</p>
        </div>
        <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-teal-700 px-4 py-3 text-sm font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-60" disabled={saveMutation.isPending} onClick={openCreateForm} type="button"><Plus aria-hidden="true" size={17} /> Add prescription</button>
      </section>

      {success && <p aria-live="polite" className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800" role="status">{success}</p>}

      {formOpen && (
        <section className="surface-card p-5 sm:p-6">
          <h3 className="text-lg font-bold text-slate-900">{editing ? 'Edit prescription' : 'Add prescription'}</h3>
          <p className="mt-1 text-sm text-slate-500">Use existing patient and doctor IDs.</p>
          <form className="mt-5 space-y-4" noValidate onSubmit={submitForm}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="text-sm font-medium text-slate-700">Patient ID<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} min="1" onChange={(event) => setForm((current) => ({ ...current, patientId: event.target.value }))} type="number" value={form.patientId} /></label>
              <label className="text-sm font-medium text-slate-700">Doctor ID<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} min="1" onChange={(event) => setForm((current) => ({ ...current, doctorId: event.target.value }))} type="number" value={form.doctorId} /></label>
              <label className="text-sm font-medium text-slate-700">Medication name<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, medicationName: event.target.value }))} value={form.medicationName} /></label>
              <label className="text-sm font-medium text-slate-700">Dosage<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, dosage: event.target.value }))} value={form.dosage} /></label>
              <label className="text-sm font-medium text-slate-700 sm:col-span-2">Instructions<textarea className="mt-1.5 min-h-28 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, instructions: event.target.value }))} value={form.instructions} /></label>
            </div>
            {formError && <p className="text-sm text-rose-700" role="alert">{formError}</p>}
            {saveMutation.isError && <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800" role="alert">{httpStatus(saveMutation.error) === 401 ? <span>Your session has expired. <Link className="font-semibold underline" onClick={signOut} to="/login">Sign in again</Link></span> : authorizationMessage(saveMutation.error) ?? errorMessage(saveMutation.error)}</div>}
            <div className="flex justify-end gap-3">
              <button className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold" disabled={saveMutation.isPending} onClick={() => setFormOpen(false)} type="button">Cancel</button>
              <button className="inline-flex items-center gap-2 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white disabled:opacity-60" disabled={saveMutation.isPending} type="submit">{saveMutation.isPending && <LoaderCircle aria-hidden="true" className="animate-spin" size={16} />}{saveMutation.isPending ? 'Saving…' : 'Save prescription'}</button>
            </div>
          </form>
        </section>
      )}

      {prescriptionsQuery.isPending ? (
        <section aria-live="polite" className="surface-card flex min-h-64 items-center justify-center gap-3 text-sm text-slate-600"><LoaderCircle aria-hidden="true" className="animate-spin text-teal-700" size={24} /> Loading prescriptions…</section>
      ) : prescriptionsQuery.isError ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center" role="alert">
          <h3 className="text-lg font-bold text-slate-900">Prescriptions could not be loaded</h3><p className="mt-2 text-sm text-slate-500">{listAuthMessage ?? errorMessage(prescriptionsQuery.error)}</p>
          {httpStatus(prescriptionsQuery.error) === 401 ? <Link className="mt-4 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white" onClick={signOut} to="/login">Sign in again</Link> : <button className="mt-4 rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold" onClick={() => void prescriptionsQuery.refetch()} type="button">Try again</button>}
        </section>
      ) : prescriptionsQuery.data.length === 0 ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center"><Pill aria-hidden="true" className="text-teal-700" size={28} /><h3 className="mt-4 text-lg font-bold text-slate-900">No prescriptions yet</h3><p className="mt-2 text-sm text-slate-500">Prescriptions added for patients will appear here.</p></section>
      ) : (
        <section className="surface-card divide-y divide-slate-100">
          <div className="px-5 py-4"><p className="text-sm font-bold text-slate-800">{prescriptionsQuery.data.length} prescriptions</p></div>
          {prescriptionsQuery.data.map((prescription) => (
            <article className="flex flex-col gap-4 px-5 py-5 sm:flex-row sm:items-start sm:justify-between sm:px-6" key={prescription.id}>
              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-x-3 gap-y-1"><h3 className="font-semibold text-slate-900">{prescription.medicationName}</h3><span className="text-xs text-slate-500">{prescription.dosage} · Patient #{prescription.patientId} · Doctor #{prescription.doctorId}</span></div>
                <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-slate-600">{prescription.instructions}</p>
                <p className="mt-2 text-xs text-slate-400">Created {formatDate(prescription.createdAt)}</p>
              </div>
              <div className="flex shrink-0 gap-2">
                <button className="rounded-lg border border-slate-200 px-3 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-50" onClick={() => openEditForm(prescription)} type="button">Edit</button>
                <button className="inline-flex items-center gap-1.5 rounded-lg border border-rose-200 px-3 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-50 disabled:opacity-60" disabled={deleteMutation.isPending} onClick={() => { if (window.confirm('Delete this prescription?')) { setSuccess(''); deleteMutation.mutate(prescription.id) } }} type="button">
                  {deleteMutation.isPending ? <LoaderCircle aria-hidden="true" className="animate-spin" size={15} /> : <Trash2 aria-hidden="true" size={15} />} Delete
                </button>
              </div>
            </article>
          ))}
          {deleteMutation.isError && <p className="border-t border-rose-100 bg-rose-50 px-5 py-3 text-sm text-rose-800" role="alert">{authorizationMessage(deleteMutation.error) ?? errorMessage(deleteMutation.error)}{httpStatus(deleteMutation.error) === 401 && <Link className="ml-2 font-semibold underline" onClick={signOut} to="/login">Sign in again</Link>}</p>}
        </section>
      )}
    </div>
  )
}
