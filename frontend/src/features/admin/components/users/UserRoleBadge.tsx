import { RoleBadge } from '../../../../components/common/RoleBadge'
import type { UserRole } from '../../../../types/auth'

export function UserRoleBadge({ role, compact = false }: { role: UserRole; compact?: boolean }) {
  return <RoleBadge role={role} compact={compact} />
}
