import { z } from 'zod'
import { RESOURCE_TYPES } from '../types/production.types'
export const stepResourceSchema = z.object({ sapCode: z.string().min(2, 'Le code est obligatoire.'), designation: z.string().min(2, 'La désignation est obligatoire.'), resourceType: z.enum(RESOURCE_TYPES), plannedQuantity: z.coerce.number().positive('La quantité prévue doit être positive.'), actualQuantity: z.coerce.number().min(0), unit: z.string().min(1, 'L’unité est obligatoire.'), unitCost: z.coerce.number().min(0), availableStock: z.coerce.number().min(0), supplier: z.string(), batchNumber: z.string(), expirationDate: z.string().nullable() })
export type StepResourceFormValues = z.infer<typeof stepResourceSchema>
