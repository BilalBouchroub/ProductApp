import { z } from 'zod'

export const loginSchema = z.object({
  email: z.email('Saisissez une adresse e-mail valide.'),
  password: z.string().min(6, 'Le mot de passe doit contenir au moins 6 caractères.'),
})

export type LoginFormValues = z.infer<typeof loginSchema>
