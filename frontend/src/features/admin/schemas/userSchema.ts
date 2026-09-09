import { z } from 'zod'
import { USER_ROLES } from '../../../types/auth'
import { USER_STATUSES } from '../types/admin.types'

const baseUserSchema = z.object({
  firstName: z.string().trim().min(2, 'Le prénom doit contenir au moins 2 caractères.').max(50, 'Le prénom est trop long.'),
  lastName: z.string().trim().min(2, 'Le nom doit contenir au moins 2 caractères.').max(50, 'Le nom est trop long.'),
  email: z.email('Saisissez une adresse e-mail valide.'),
  phoneNumber: z.string().trim().min(10, 'Saisissez un numéro de téléphone valide.').max(25, 'Le numéro est trop long.'),
  role: z.enum(USER_ROLES),
  status: z.enum(USER_STATUSES),
})

const strongPassword = z.string()
  .min(12, 'Le mot de passe doit contenir au moins 12 caractères.')
  .regex(/[A-Z]/, 'Ajoutez au moins une lettre majuscule.')
  .regex(/[a-z]/, 'Ajoutez au moins une lettre minuscule.')
  .regex(/[0-9]/, 'Ajoutez au moins un chiffre.')
  .regex(/[^a-zA-Z0-9]/, 'Ajoutez au moins un caractère spécial.')

export const userSchema = baseUserSchema.extend({
  temporaryPassword: z.string().optional(),
  confirmTemporaryPassword: z.string().optional(),
})

export const createUserSchema = baseUserSchema.extend({
  temporaryPassword: strongPassword,
  confirmTemporaryPassword: z.string().min(1, 'Confirmez le mot de passe temporaire.'),
}).refine(values => values.temporaryPassword === values.confirmTemporaryPassword, {
  path: ['confirmTemporaryPassword'],
  message: 'Les mots de passe ne correspondent pas.',
})

export type UserFormValues = z.infer<typeof userSchema>
