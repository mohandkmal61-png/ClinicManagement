# ClinicManagement.Web

The React frontend workspace for the ClinicManagement REST API.

## Current scope

This initial scaffold provides the responsive application shell and routes for the future clinic-management screens. The Login, Register, Dashboard, Patients, Doctors, Appointments, Medical Records, and Prescriptions pages are placeholders only. Authentication, API calls, CRUD workflows, and role-based route protection have not been implemented.

## Technology

- React and TypeScript with Vite
- React Router
- Tailwind CSS v4 via the Vite plugin
- Axios client foundation
- TanStack Query provider
- React Hook Form and Zod dependencies for future forms
- Lucide React icons

## Setup

Install dependencies and start the development server:

```powershell
npm install
npm run dev
```

The API base URL can be configured using `.env.local`, based on `.env.example`. Do not put secrets or tokens in Vite environment variables; client-side environment values are bundled into browser assets.

Create a production build:

```powershell
npm run build
```
