import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { AdminUser } from '../types/admin.types'
import { UsersPage } from './UsersPage'

const mocks = vi.hoisted(() => ({
  remove: vi.fn(),
  status: vi.fn(),
  role: vi.fn(),
  password: vi.fn(),
}))

const alice: AdminUser = {
  id: '10000000-0000-0000-0000-000000000001',
  firstName: 'Alice', lastName: 'Martin', fullName: 'Alice Martin',
  email: 'alice@productapp.local', phoneNumber: '', role: 'ProductionManager', status: 'Active',
  createdAt: '2026-01-01T00:00:00Z', updatedAt: '2026-01-01T00:00:00Z', lastLoginAt: null, avatarUrl: null,
}

vi.mock('../hooks/useAdminUsers', () => ({
  useAdminUsers: () => ({ data: [alice], isLoading: false, isError: false, refetch: vi.fn() }),
  useAdminUserMutations: () => ({
    deleteUser: { mutateAsync: mocks.remove, isPending: false },
    setStatus: { mutateAsync: mocks.status, isPending: false, variables: undefined },
    changeRole: { mutateAsync: mocks.role, isPending: false },
    resetPassword: { mutateAsync: mocks.password, isPending: false, variables: undefined },
  }),
}))

describe('UsersPage click workflow', () => {
  beforeEach(() => {
    Object.values(mocks).forEach(mock => mock.mockReset())
    mocks.remove.mockResolvedValue(undefined)
    mocks.status.mockResolvedValue(alice)
    mocks.role.mockResolvedValue(alice)
    mocks.password.mockResolvedValue(undefined)
  })

  it('confirme puis exécute la suppression sans superposition plein écran', async () => {
    const user = userEvent.setup()
    render(<MemoryRouter><UsersPage /></MemoryRouter>)

    await user.click(screen.getByRole('button', { name: 'Supprimer Alice Martin' }))
    expect(screen.getByRole('dialog', { name: 'Confirmation de l’action' })).toBeVisible()
    expect(document.querySelector('.fixed.inset-0')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Supprimer maintenant' }))
    await waitFor(() => expect(mocks.remove).toHaveBeenCalledWith(alice.id))
  })

  it('exécute la désactivation et la réinitialisation depuis les clics utilisateur', async () => {
    const user = userEvent.setup()
    render(<MemoryRouter><UsersPage /></MemoryRouter>)

    await user.click(screen.getByRole('button', { name: 'Désactiver Alice Martin' }))
    await user.click(screen.getByRole('button', { name: 'Désactiver maintenant' }))
    await waitFor(() => expect(mocks.status).toHaveBeenCalledWith({ id: alice.id, status: 'Inactive' }))

    await user.click(screen.getByRole('button', { name: 'Réinitialiser le mot de passe de Alice Martin' }))
    await waitFor(() => expect(mocks.password).toHaveBeenCalledWith(alice.id))
  })
})
