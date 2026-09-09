import { beforeEach, describe, expect, it, vi } from 'vitest'
import { usersApi } from './usersApi'

const mocks = vi.hoisted(() => ({ post: vi.fn() }))

vi.mock('./axiosClient', () => ({
  axiosClient: {
    post: mocks.post,
    get: vi.fn(),
    put: vi.fn(),
  },
}))

const responseUser = {
  id: '10000000-0000-0000-0000-000000000001',
  firstName: 'Alice', lastName: 'Martin', fullName: 'Alice Martin', email: 'alice@productapp.local',
  phoneNumber: null, role: 'ProductionManager' as const, status: 'Active' as const,
  createdAt: '2026-01-01T00:00:00Z', updatedAt: '2026-01-01T00:00:00Z', lastLoginAt: null,
}

describe('usersApi administration actions', () => {
  beforeEach(() => {
    mocks.post.mockReset()
    mocks.post.mockResolvedValue({ data: { deleted: false, user: responseUser } })
  })

  it.each([
    ['Active', 'Activate'],
    ['Inactive', 'Deactivate'],
    ['Suspended', 'Suspend'],
  ] as const)('envoie le statut %s via l’action %s', async (status, action) => {
    await usersApi.setStatus(responseUser.id, status)
    expect(mocks.post).toHaveBeenCalledWith(`/users/${responseUser.id}/actions`, { action, role: undefined })
  })

  it('envoie rôle, suppression et mot de passe vers le même endpoint', async () => {
    await usersApi.changeRole(responseUser.id, 'Administrator')
    await usersApi.remove(responseUser.id)
    await usersApi.requestPasswordReset(responseUser.id)

    expect(mocks.post).toHaveBeenNthCalledWith(1, `/users/${responseUser.id}/actions`, { action: 'ChangeRole', role: 'Administrator' })
    expect(mocks.post).toHaveBeenNthCalledWith(2, `/users/${responseUser.id}/actions`, { action: 'Delete', role: undefined })
    expect(mocks.post).toHaveBeenNthCalledWith(3, `/users/${responseUser.id}/actions`, { action: 'ResetPassword', role: undefined })
  })
})
