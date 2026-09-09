import { format } from 'date-fns'
import { fr } from 'date-fns/locale'

export function formatAdminDate(value: string | null): string {
  return value ? format(new Date(value), 'dd MMM yyyy', { locale: fr }) : 'Jamais'
}

export function formatAdminDateTime(value: string): string {
  return format(new Date(value), 'dd MMM yyyy · HH:mm', { locale: fr })
}

export function getInitials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase()
}
