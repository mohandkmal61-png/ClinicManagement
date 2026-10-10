import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { FileHeart, LoaderCircle, Plus, Trash2 } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { apiClient } from '../services/apiClient'

interface MedicalRecord {
  id: number
  patientId: number
  doctorId: number
  diagnosis: string
  notes: string
  createdAt: string
}

interface MedicalRecordForm {
  patientId: string
  doctorId: string
  diagnosis: string
  notes: string
}

const emptyForm: MedicalRecordForm = { patientId: '', doctorId: '', diagnosis: '', notes: '' }

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
  if (httpStatus(error) === 403) return 'Your account does not have permission to access medical records.'
  return null
}

function formatDate(value: string): string {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date)
}

export function MedicalRecordsPage() {
  const { signOut } = useAuth()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<MedicalRecord | null>(null)
  const [form, setForm] = useState<MedicalRecordForm>(emptyForm)
  const [formError, setFormError] = useState('')
  const [success, setSuccess] = useState('')

  const recordsQuery = useQuery({
    queryKey: ['medicalRecords'],
    queryFn: async () => (await apiClient.get<MedicalRecord[]>('/api/MedicalRecords')).data,
  })

  const saveMutation = useMutation({
    mutationFn: async (input: { id: number | null; values: MedicalRecordForm }) => {
      const body = {
        patientId: Number(input.values.patientId),
        doctorId: Number(input.values.doctorId),
        diagnosis: input.values.diagnosis.trim(),
        notes: input.values.notes.trim(),
      }
      if (input.id === null) return (await apiClient.post<MedicalRecord>('/api/MedicalRecords', body)).data
      await apiClient.put(`/api/MedicalRecords/${input.id}`, body)
    },
    onSuccess: async () => {
      setSuccess(editing ? 'Medical record updated successfully.' : 'Medical record added successfully.')
      setFormOpen(false)
      setEditing(null)
      setForm(emptyForm)
      await queryClient.invalidateQueries({ queryKey: ['medicalRecords'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => apiClient.delete(`/api/MedicalRecords/${id}`),
    onSuccess: async () => {
      setSuccess('Medical record deleted successfully.')
      await queryClient.invalidateQueries({ queryKey: ['medicalRecords'] })
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

  function openEditForm(record: MedicalRecord) {
    setSuccess('')
    setEditing(record)
    setForm({ patientId: String(record.patientId), doctorId: String(record.doctorId), diagnosis: record.diagnosis, notes: record.notes })
    setFormError('')
    saveMutation.reset()
    setFormOpen(true)
  }

  function submitForm(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const patientId = Number(form.patientId)
    const doctorId = Number(form.doctorId)
    if (!Number.isInteger(patientId) || patientId < 1 || !Number.isInteger(doctorId) || doctorId < 1 ||
        !form.diagnosis.trim() || !form.notes.trim()) {
      setFormError('Enter valid patient and doctor IDs, diagnosis, and notes.')
      return
    }
    setFormError('')
    setSuccess('')
    saveMutation.mutate({ id: editing?.id ?? null, values: form })
  }

  const listAuthMessage = recordsQuery.isError ? authorizationMessage(recordsQuery.error) : null

  return (
    <div className="space-y-6">
      <section className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.14em] text-teal-700"><FileHeart aria-hidden="true" size={15} /> Patient care</div>
          <h2 className="mt-2 text-2xl font-bold tracking-tight text-slate-900">Medical records</h2>
          <p className="mt-1 text-sm text-slate-500">Review diagnoses and notes recorded for patients.</p>
        </div>
        <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-teal-700 px-4 py-3 text-sm font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-60" disabled={saveMutation.isPending} onClick={openCreateForm} type="button"><Plus aria-hidden="true" size={17} /> Add record</button>
      </section>

      {success && <p aria-live="polite" className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800" role="status">{success}</p>}

      {formOpen && (
        <section className="surface-card p-5 sm:p-6">
          <h3 className="text-lg font-bold text-slate-900">{editing ? 'Edit medical record' : 'Add medical record'}</h3>
          <p className="mt-1 text-sm text-slate-500">Use existing patient and doctor IDs.</p>
          <form className="mt-5 space-y-4" noValidate onSubmit={submitForm}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="text-sm font-medium text-slate-700">Patient ID<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} min="1" onChange={(event) => setForm((current) => ({ ...current, patientId: event.target.value }))} type="number" value={form.patientId} /></label>
              <label className="text-sm font-medium text-slate-700">Doctor ID<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} min="1" onChange={(event) => setForm((current) => ({ ...current, doctorId: event.target.value }))} type="number" value={form.doctorId} /></label>
              <label className="text-sm font-medium text-slate-700 sm:col-span-2">Diagnosis<input className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, diagnosis: event.target.value }))} value={form.diagnosis} /></label>
              <label className="text-sm font-medium text-slate-700 sm:col-span-2">Notes<textarea className="mt-1.5 min-h-28 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10" disabled={saveMutation.isPending} onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))} value={form.notes} /></label>
            </div>
            {formError && <p className="text-sm text-rose-700" role="alert">{formError}</p>}
            {saveMutation.isError && <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800" role="alert">{httpStatus(saveMutation.error) === 401 ? <span>Your session has expired. <Link className="font-semibold underline" onClick={signOut} to="/login">Sign in again</Link></span> : authorizationMessage(saveMutation.error) ?? errorMessage(saveMutation.error)}</div>}
            <div className="flex justify-end gap-3">
              <button className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold" disabled={saveMutation.isPending} onClick={() => setFormOpen(false)} type="button">Cancel</button>
              <button className="inline-flex items-center gap-2 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white disabled:opacity-60" disabled={saveMutation.isPending} type="submit">{saveMutation.isPending && <LoaderCircle aria-hidden="true" className="animate-spin" size={16} />}{saveMutation.isPending ? 'Saving…' : 'Save record'}</button>
            </div>
          </form>
        </section>
      )}

      {recordsQuery.isPending ? (
        <section aria-live="polite" className="surface-card flex min-h-64 items-center justify-center gap-3 text-sm text-slate-600"><LoaderCircle aria-hidden="true" className="animate-spin text-teal-700" size={24} /> Loading medical records…</section>
      ) : recordsQuery.isError ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center" role="alert">
          <h3 className="text-lg font-bold text-slate-900">Medical records could not be loaded</h3><p className="mt-2 text-sm text-slate-500">{listAuthMessage ?? errorMessage(recordsQuery.error)}</p>
          {httpStatus(recordsQuery.error) === 401 ? <Link className="mt-4 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white" onClick={signOut} to="/login">Sign in again</Link> : <button className="mt-4 rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold" onClick={() => void recordsQuery.refetch()} type="button">Try again</button>}
        </section>
      ) : recordsQuery.data.length === 0 ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center"><FileHeart aria-hidden="true" className="text-teal-700" size={28} /><h3 className="mt-4 text-lg font-bold text-slate-900">No medical records yet</h3><p className="mt-2 text-sm text-slate-500">Records added for patients will appear here.</p></section>
      ) : (
        <section className="surface-card divide-y divide-slate-100">
          <div className="px-5 py-4"><p className="text-sm font-bold text-slate-800">{recordsQuery.data.length} medical records</p></div>
          {recordsQuery.data.map((record) => (
            <article className="flex flex-col gap-4 px-5 py-5 sm:flex-row sm:items-start sm:justify-between sm:px-6" key={record.id}>
              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-x-3 gap-y-1"><h3 className="font-semibold text-slate-900">{record.diagnosis}</h3><span className="text-xs text-slate-500">Patient #{record.patientId} · Doctor #{record.doctorId}</span></div>
                <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-slate-600">{record.notes}</p>
                <p className="mt-2 text-xs text-slate-400">Created {formatDate(record.createdAt)}</p>
              </div>
              <div className="flex shrink-0 gap-2">
                <button className="rounded-lg border border-slate-200 px-3 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-50" onClick={() => openEditForm(record)} type="button">Edit</button>
                <button className="inline-flex items-center gap-1.5 rounded-lg border border-rose-200 px-3 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-50 disabled:opacity-60" disabled={deleteMutation.isPending} onClick={() => { if (window.confirm('Delete this medical record?')) { setSuccess(''); deleteMutation.mutate(record.id) } }} type="button">
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
