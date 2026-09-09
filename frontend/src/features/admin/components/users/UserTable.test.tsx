import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import type { AdminUser } from '../../types/admin.types'
import { UserTable } from './UserTable'

const alice: AdminUser = {
  id: '10000000-0000-0000-0000-000000000001',
  firstName: 'Alice',
  lastName: 'Martin',
  fullName: 'Alice Martin',
  email: 'alice@productapp.local',
  phoneNumber: '',
  role: 'ProductionManager',
  status: 'Active',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
  lastLoginAt: null,
  avatarUrl: null,
}

function renderTable() {
  const handlers = {
    onDelete: vi.fn(),
    onToggleStatus: vi.fn(),
    onChangeRole: vi.fn(),
    onResetPassword: vi.fn(),
  }
  render(<MemoryRouter><UserTable users={[alice]} {...handlers} /></MemoryRouter>)
  return handlers
}

describe('UserTable actions', () => {
  it('expose des destinations valides pour voir et modifier', () => {
    renderTable()
    expect(screen.getByRole('link', { name: 'Consulter Alice Martin' })).toHaveAttribute('href', `/admin/users/${alice.id}`)
    expect(screen.getByRole('link', { name: 'Modifier Alice Martin' })).toHaveAttribute('href', `/admin/users/${alice.id}/edit`)
  })

  it('déclenche chaque action avec le bon utilisateur', async () => {
    const user = userEvent.setup()
    const handlers = renderTable()

    await user.click(screen.getByRole('button', { name: 'Désactiver Alice Martin' }))
    await user.click(screen.getByRole('button', { name: 'Changer le rôle de Alice Martin' }))
    await user.click(screen.getByRole('button', { name: 'Réinitialiser le mot de passe de Alice Martin' }))
    await user.click(screen.getByRole('button', { name: 'Supprimer Alice Martin' }))

    expect(handlers.onToggleStatus).toHaveBeenCalledWith(alice)
    expect(handlers.onChangeRole).toHaveBeenCalledWith(alice)
    expect(handlers.onResetPassword).toHaveBeenCalledWith(alice)
    expect(handlers.onDelete).toHaveBeenCalledWith(alice)
  })

  it('laisse les autres actions cliquables pendant une opération en cours', async () => {
    const user = userEvent.setup()
    const onDelete = vi.fn()
    const onToggleStatus = vi.fn()
    render(<MemoryRouter><UserTable
      users={[alice]}
      pendingAction={{ id: alice.id, action: 'password' }}
      onDelete={onDelete}
      onToggleStatus={onToggleStatus}
      onChangeRole={vi.fn()}
      onResetPassword={vi.fn()}
    /></MemoryRouter>)

    expect(screen.getByRole('button', { name: 'Réinitialiser le mot de passe de Alice Martin' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Supprimer Alice Martin' })).toBeEnabled()
    expect(screen.getByRole('button', { name: 'Désactiver Alice Martin' })).toBeEnabled()

    await user.click(screen.getByRole('button', { name: 'Supprimer Alice Martin' }))
    await user.click(screen.getByRole('button', { name: 'Désactiver Alice Martin' }))
    expect(onDelete).toHaveBeenCalledWith(alice)
    expect(onToggleStatus).toHaveBeenCalledWith(alice)
  })
})
