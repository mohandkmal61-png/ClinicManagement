import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { LoaderCircle, Plus, Stethoscope, Trash2, UserRoundPen } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { apiClient } from '../services/apiClient'

interface Doctor {
  id: number
  name: string
  specialization: string
}

interface DoctorForm {
  name: string
  specialization: string
}

interface ApiErrorBody {
  title?: unknown
  detail?: unknown
  errors?: unknown
}

const emptyForm: DoctorForm = { name: '', specialization: '' }

function httpStatus(error: unknown): number | undefined {
  return axios.isAxiosError(error) ? error.response?.status : undefined
}

function errorMessage(error: unknown): string {
  if (axios.isAxiosError<ApiErrorBody>(error)) {
    const body = error.response?.data
    if (body && typeof body === 'object') {
      if (typeof body.detail === 'string' && body.detail.trim()) return body.detail
      if (typeof body.title === 'string' && body.title.trim()) return body.title
      if (body.errors && typeof body.errors === 'object') {
        const messages = Object.values(body.errors).flatMap((value) =>
          Array.isArray(value) ? value.filter((message): message is string => typeof message === 'string') : [],
        )
        if (messages.length) return messages.join(' ')
      }
    }
  }
  return 'The request could not be completed. Please try again.'
}

function authorizationMessage(error: unknown, resource: string): string | null {
  const status = httpStatus(error)
  if (status === 401) return 'Your session has expired or is invalid. Sign in again to continue.'
  if (status === 403) return `Your account does not have permission to access ${resource}.`
  return null
}

export function DoctorsPage() {
  const { signOut } = useAuth()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editingDoctor, setEditingDoctor] = useState<Doctor | null>(null)
  const [form, setForm] = useState<DoctorForm>(emptyForm)
  const [formError, setFormError] = useState('')
  const [success, setSuccess] = useState('')

  const doctorsQuery = useQuery({
    queryKey: ['doctors'],
    queryFn: async () => (await apiClient.get<Doctor[]>('/api/Doctors')).data,
  })

  const saveMutation = useMutation({
    mutationFn: async (input: { id: number | null; values: DoctorForm }) => {
      if (input.id === null) {
        return (await apiClient.post<Doctor>('/api/Doctors', input.values)).data
      }
      await apiClient.put(`/api/Doctors/${input.id}`, input.values)
    },
    onSuccess: async () => {
      setSuccess(editingDoctor ? 'Doctor updated successfully.' : 'Doctor added successfully.')
      setFormOpen(false)
      setEditingDoctor(null)
      setForm(emptyForm)
      setFormError('')
      await queryClient.invalidateQueries({ queryKey: ['doctors'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => apiClient.delete(`/api/Doctors/${id}`),
    onSuccess: async () => {
      setSuccess('Doctor deleted successfully.')
      await queryClient.invalidateQueries({ queryKey: ['doctors'] })
    },
  })

  function openCreateForm() {
    setSuccess('')
    setFormError('')
    setEditingDoctor(null)
    setForm(emptyForm)
    saveMutation.reset()
    setFormOpen(true)
  }

  function openEditForm(doctor: Doctor) {
    setSuccess('')
    setFormError('')
    setEditingDoctor(doctor)
    setForm({ name: doctor.name, specialization: doctor.specialization })
    saveMutation.reset()
    setFormOpen(true)
  }

  function submitForm(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!form.name.trim() || !form.specialization.trim()) {
      setFormError('Name and specialization are required.')
      return
    }
    setFormError('')
    setSuccess('')
    saveMutation.mutate({
      id: editingDoctor?.id ?? null,
      values: { name: form.name.trim(), specialization: form.specialization.trim() },
    })
  }

  const listAuthMessage = doctorsQuery.isError
    ? authorizationMessage(doctorsQuery.error, 'the doctors directory')
    : null

  return (
    <div className="space-y-6">
      <section className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.14em] text-teal-700">
            <Stethoscope aria-hidden="true" size={15} />
            Care team
          </div>
          <h2 className="mt-2 text-2xl font-bold tracking-tight text-slate-900">Doctors</h2>
          <p className="mt-1 text-sm text-slate-500">Manage doctors and their specializations.</p>
        </div>
        <button
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-teal-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-60"
          disabled={saveMutation.isPending}
          onClick={openCreateForm}
          type="button"
        >
          <Plus aria-hidden="true" size={17} />
          Add Doctor
        </button>
      </section>

      {success && <p aria-live="polite" className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800" role="status">{success}</p>}

      {formOpen && (
        <section className="surface-card p-5 sm:p-6">
          <h3 className="text-lg font-bold text-slate-900">{editingDoctor ? 'Edit doctor' : 'Add a doctor'}</h3>
          <form className="mt-5 space-y-4" noValidate onSubmit={submitForm}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="text-sm font-medium text-slate-700">
                Name
                <input
                  autoComplete="name"
                  className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10"
                  disabled={saveMutation.isPending}
                  onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
                  value={form.name}
                />
              </label>
              <label className="text-sm font-medium text-slate-700">
                Specialization
                <input
                  className="mt-1.5 w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-teal-600 focus:ring-4 focus:ring-teal-600/10"
                  disabled={saveMutation.isPending}
                  onChange={(event) => setForm((current) => ({ ...current, specialization: event.target.value }))}
                  value={form.specialization}
                />
              </label>
            </div>
            {formError && <p className="text-sm text-rose-700" role="alert">{formError}</p>}
            {saveMutation.isError && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800" role="alert">
                {httpStatus(saveMutation.error) === 401 ? (
                  <span>Your session has expired. <Link className="font-semibold underline" onClick={signOut} to="/login">Sign in again</Link></span>
                ) : authorizationMessage(saveMutation.error, 'manage doctors') ?? errorMessage(saveMutation.error)}
              </div>
            )}
            <div className="flex justify-end gap-3">
              <button className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-700" disabled={saveMutation.isPending} onClick={() => setFormOpen(false)} type="button">Cancel</button>
              <button className="inline-flex items-center gap-2 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white disabled:opacity-60" disabled={saveMutation.isPending} type="submit">
                {saveMutation.isPending && <LoaderCircle aria-hidden="true" className="animate-spin" size={16} />}
                {saveMutation.isPending ? 'Saving…' : 'Save Doctor'}
              </button>
            </div>
          </form>
        </section>
      )}

      {doctorsQuery.isPending ? (
        <section aria-live="polite" className="surface-card flex min-h-64 items-center justify-center gap-3 text-sm text-slate-600">
          <LoaderCircle aria-hidden="true" className="animate-spin text-teal-700" size={24} /> Loading doctors…
        </section>
      ) : doctorsQuery.isError ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center" role="alert">
          <h3 className="text-lg font-bold text-slate-900">Doctors could not be loaded</h3>
          <p className="mt-2 text-sm text-slate-500">{listAuthMessage ?? errorMessage(doctorsQuery.error)}</p>
          {httpStatus(doctorsQuery.error) === 401 ? (
            <Link className="mt-4 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white" onClick={signOut} to="/login">Sign in again</Link>
          ) : (
            <button className="mt-4 rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold" onClick={() => void doctorsQuery.refetch()} type="button">Try again</button>
          )}
        </section>
      ) : doctorsQuery.data.length === 0 ? (
        <section className="surface-card flex min-h-64 flex-col items-center justify-center px-6 text-center">
          <Stethoscope aria-hidden="true" className="text-teal-700" size={28} />
          <h3 className="mt-4 text-lg font-bold text-slate-900">No doctors yet</h3>
          <p className="mt-2 text-sm text-slate-500">Add a doctor to start building the care team directory.</p>
        </section>
      ) : (
        <section className="surface-card overflow-hidden">
          <div className="border-b border-slate-100 px-5 py-4 sm:px-6">
            <p className="text-sm font-bold text-slate-800">{doctorsQuery.data.length} doctor{doctorsQuery.data.length === 1 ? '' : 's'}</p>
          </div>
          <div className="divide-y divide-slate-100">
            {doctorsQuery.data.map((doctor) => (
              <article className="flex flex-col gap-4 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6" key={doctor.id}>
                <div className="flex items-center gap-3">
                  <span className="flex size-10 items-center justify-center rounded-full bg-teal-50 text-teal-800"><Stethoscope aria-hidden="true" size={18} /></span>
                  <div>
                    <h3 className="font-semibold text-slate-800">{doctor.name}</h3>
                    <p className="text-sm text-slate-500">{doctor.specialization}</p>
                  </div>
                </div>
                <div className="flex gap-2">
                  <button className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-50" onClick={() => openEditForm(doctor)} type="button">
                    <UserRoundPen aria-hidden="true" size={15} /> Edit
                  </button>
                  <button
                    className="inline-flex items-center gap-1.5 rounded-lg border border-rose-200 px-3 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-50 disabled:opacity-60"
                    disabled={deleteMutation.isPending}
                    onClick={() => {
                      if (window.confirm(`Delete ${doctor.name}?`)) {
                        setSuccess('')
                        deleteMutation.mutate(doctor.id)
                      }
                    }}
                    type="button"
                  >
                    {deleteMutation.isPending ? <LoaderCircle aria-hidden="true" className="animate-spin" size={15} /> : <Trash2 aria-hidden="true" size={15} />}
                    Delete
                  </button>
                </div>
              </article>
            ))}
          </div>
          {deleteMutation.isError && (
            <p className="border-t border-rose-100 bg-rose-50 px-5 py-3 text-sm text-rose-800" role="alert">
              {authorizationMessage(deleteMutation.error, 'delete doctors') ?? errorMessage(deleteMutation.error)}
              {httpStatus(deleteMutation.error) === 401 && <Link className="ml-2 font-semibold underline" onClick={signOut} to="/login">Sign in again</Link>}
            </p>
          )}
        </section>
      )}
    </div>
  )
}
