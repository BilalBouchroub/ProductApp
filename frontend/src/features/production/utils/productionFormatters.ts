import { format } from 'date-fns'
import { fr } from 'date-fns/locale'
export function formatProductionDate(value: string | null): string { return value ? format(new Date(value), 'dd MMM yyyy', { locale: fr }) : 'En cours' }
export function formatCurrency(value: number): string { return new Intl.NumberFormat('fr-MA', { style: 'currency', currency: 'MAD', maximumFractionDigits: 2 }).format(value) }
export function formatQuantity(value: number, unit: string): string { return `${new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 2 }).format(value)} ${unit}` }
