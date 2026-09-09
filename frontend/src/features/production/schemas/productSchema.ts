import { z } from 'zod'
import { PRODUCT_STATUSES, PRODUCTION_UNITS } from '../types/production.types'

export const productSchema = z.object({
  name: z.string().trim().min(3, 'Le nom doit contenir au moins 3 caracteres.'),
  internalReference: z.string().trim().min(3, 'La reference interne est obligatoire.'),
  sapCode: z.string().trim().min(3, 'Le code SAP est obligatoire.'),
  category: z.string().trim().min(2, 'La categorie est obligatoire.'),
  description: z.string().trim().min(10, 'Decrivez le produit en quelques mots.'),
  imageUrl: z.string().trim().url('URL invalide.').nullable().or(z.literal('').transform(() => null)),
  themeColor: z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Couleur invalide.'),
  targetSalePrice: z.coerce.number().positive('Le prix cible doit etre positif.'),
  batchQuantity: z.coerce.number().positive('La quantite du lot doit etre positive.'),
  productionUnit: z.enum(PRODUCTION_UNITS),
  status: z.enum(PRODUCT_STATUSES),
})

export type ProductFormValues = z.infer<typeof productSchema>
