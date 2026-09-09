import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import type { UserRole } from '../types/auth'

export function RoleRoute({ allowedRoles }: { allowedRoles: readonly UserRole[] }) {
  const { session } = useAuth()
  if (!session) return <Navigate to="/login" replace />
  return allowedRoles.includes(session.user.role) ? <Outlet /> : <Navigate to="/unauthorized" replace />
}
