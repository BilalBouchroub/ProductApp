import { Check } from 'lucide-react'

export function PermissionList({ permissions }: { permissions: readonly string[] }) {
  return <ul className="space-y-3">{permissions.map((permission) => <li key={permission} className="flex items-start gap-3 text-sm leading-5 text-slate-600"><span className="mt-0.5 grid size-5 shrink-0 place-items-center rounded-full bg-emerald-50 text-emerald-600"><Check className="size-3" /></span>{permission}</li>)}</ul>
}
