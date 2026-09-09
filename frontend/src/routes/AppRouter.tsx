import { lazy, Suspense, type ComponentType } from 'react'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { LoadingSkeleton } from '../components/ui/LoadingSkeleton'
import { LoginPage } from '../features/auth/pages/LoginPage'
import { AppLayout } from '../layouts/AppLayout'
import { AuthLayout } from '../layouts/AuthLayout'
import { NotFoundPage } from '../pages/NotFoundPage'
import { UnauthorizedPage } from '../pages/UnauthorizedPage'
import { ProtectedRoute } from './ProtectedRoute'
import { RoleRoute } from './RoleRoute'

function page<T>(loader: () => Promise<T>, key: keyof T) {
  return lazy(() => loader().then(module => ({ default: module[key] as ComponentType })))
}

const SmartProductPage = page(() => import('../features/ai/pages/SmartProductPage'), 'SmartProductPage')
const LandingPage = page(() => import('../features/landing/pages/LandingPage'), 'LandingPage')

const adminRoutes = [
  ['/admin/dashboard', page(() => import('../features/admin/pages/AdminDashboardPage'), 'AdminDashboardPage')],
  ['/admin/users', page(() => import('../features/admin/pages/UsersPage'), 'UsersPage')],
  ['/admin/users/new', page(() => import('../features/admin/pages/UserCreatePage'), 'UserCreatePage')],
  ['/admin/users/:id', page(() => import('../features/admin/pages/UserDetailsPage'), 'UserDetailsPage')],
  ['/admin/users/:id/edit', page(() => import('../features/admin/pages/UserEditPage'), 'UserEditPage')],
  ['/admin/roles', page(() => import('../features/admin/pages/RolesPage'), 'RolesPage')],
  ['/admin/logs', page(() => import('../features/admin/pages/LogsPage'), 'LogsPage')],
] as const

const productionRoutes = [
  ['/production/dashboard', page(() => import('../features/production/pages/ProductionDashboardPage'), 'ProductionDashboardPage')],
  ['/production/products', page(() => import('../features/production/pages/ProductsPage'), 'ProductsPage')],
  ['/production/products/new', page(() => import('../features/production/pages/ProductCreatePage'), 'ProductCreatePage')],
  ['/production/products/:id', page(() => import('../features/production/pages/ProductDetailsPage'), 'ProductDetailsPage')],
  ['/production/products/:id/edit', page(() => import('../features/production/pages/ProductEditPage'), 'ProductEditPage')],
  ['/production/products/:id/process', page(() => import('../features/production/pages/ProductionProcessBuilderPage'), 'ProductionProcessBuilderPage')],
  ['/production/experiments', page(() => import('../features/production/pages/ExperimentsPage'), 'ExperimentsPage')],
  ['/production/experiments/new', page(() => import('../features/production/pages/ExperimentCreatePage'), 'ExperimentCreatePage')],
  ['/production/experiments/recent', page(() => import('../features/production/pages/RecentExperimentsPage'), 'RecentExperimentsPage')],
  ['/production/experiments/:id', page(() => import('../features/production/pages/ExperimentDetailsPage'), 'ExperimentDetailsPage')],
  ['/production/optimization-requests', page(() => import('../features/production/pages/OptimizationRequestsPage'), 'OptimizationRequestsPage')],
] as const

const commercialRoutes = [
  ['/commercial/dashboard', page(() => import('../features/commercial/pages/CommercialDashboardPage'), 'CommercialDashboardPage')],
  ['/commercial/products', page(() => import('../features/commercial/pages/CommercialProductsPage'), 'CommercialProductsPage')],
  ['/commercial/products/:productId', page(() => import('../features/commercial/pages/CommercialProductDetailsPage'), 'CommercialProductDetailsPage')],
  ['/commercial/products/:productId/market-study', page(() => import('../features/commercial/pages/MarketStudyWizardPage'), 'MarketStudyWizardPage')],
  ['/commercial/studies', page(() => import('../features/commercial/pages/MarketStudiesPage'), 'MarketStudiesPage')],
  ['/commercial/studies/:studyId', page(() => import('../features/commercial/pages/MarketStudyDetailsPage'), 'MarketStudyDetailsPage')],
  ['/commercial/studies/:studyId/result', page(() => import('../features/commercial/pages/MarketStudyResultPage'), 'MarketStudyResultPage')],
  ['/commercial/comparison', page(() => import('../features/commercial/pages/CommercialComparisonPage'), 'CommercialComparisonPage')],
  ['/commercial/reports', page(() => import('../features/commercial/pages/CommercialReportsPage'), 'CommercialReportsPage')],
] as const

function renderRoutes(routes: ReadonlyArray<readonly [string, ComponentType]>) {
  return routes.map(([path, Component]) => <Route key={path} path={path} element={<Component />} />)
}

export function AppRouter() {
  return (
    <BrowserRouter>
      <Suspense fallback={<div className="mx-auto max-w-4xl p-8"><LoadingSkeleton lines={8} /></div>}>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route element={<AuthLayout />}>
            <Route path="/login" element={<LoginPage />} />
          </Route>
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route path="/ai" element={<SmartProductPage />} />
              <Route path="/ai/:conversationId" element={<SmartProductPage />} />
              <Route element={<RoleRoute allowedRoles={['Administrator']} />}>
                {renderRoutes(adminRoutes)}
              </Route>
              <Route element={<RoleRoute allowedRoles={['ProductionManager']} />}>
                {renderRoutes(productionRoutes)}
              </Route>
              <Route element={<RoleRoute allowedRoles={['CommercialManager']} />}>
                {renderRoutes(commercialRoutes)}
              </Route>
            </Route>
          </Route>
          <Route path="/unauthorized" element={<UnauthorizedPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  )
}
