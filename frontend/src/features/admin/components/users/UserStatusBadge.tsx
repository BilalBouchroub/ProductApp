import { StatusBadge } from '../../../../components/common/StatusBadge'
import type { UserStatus } from '../../types/admin.types'

const settings = {
  Active: { label: 'Actif', tone: 'success' },
  Inactive: { label: 'Inactif', tone: 'neutral' },
  Suspended: { label: 'Suspendu', tone: 'danger' },
} as const

export function UserStatusBadge({ status }: { status: UserStatus }) {
  const setting = settings[status]
  return <StatusBadge label={setting.label} tone={setting.tone} />
}
