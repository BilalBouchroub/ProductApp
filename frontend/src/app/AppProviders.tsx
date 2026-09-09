import { QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'sonner'
import { useEffect, type ReactNode } from 'react'
import { AuthProvider } from '../store/AuthProvider'
import { queryClient } from './queryClient'
import { useApiMocks } from '../api/apiMode'

function DemoDataBridge() {
  useEffect(() => {
    if (!useApiMocks) return undefined
    let unsubscribe: (() => void) | undefined
    void import('../mocks/shared/sharedMock.repository').then(({ sharedMockRepository }) => {
      unsubscribe = sharedMockRepository.subscribe(() => { void queryClient.invalidateQueries() })
    })
    return () => unsubscribe?.()
  }, [])
  return null
}

export function AppProviders({ children }: { children: ReactNode }) {
  return <QueryClientProvider client={queryClient}>
    <DemoDataBridge />
    <AuthProvider>{children}<Toaster richColors position="top-right" closeButton /></AuthProvider>
  </QueryClientProvider>
}
