import { formatDistanceToNow } from 'date-fns'
import { fr } from 'date-fns/locale'

export function formatRelativeDate(value: string): string {
  return formatDistanceToNow(new Date(value), { addSuffix: true, locale: fr })
}

export function formatNumber(value: number): string {
  return new Intl.NumberFormat('fr-FR').format(value)
}
