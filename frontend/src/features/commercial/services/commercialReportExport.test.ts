import { describe, expect, it } from 'vitest'
import { buildExecutiveReportHtml, buildPortfolioCsv } from './commercialReportExport'
import type { MarketStudy } from '../types/commercial.types'
import { defaultStudyInput } from '../mocks/marketStudies.mock'

const study: MarketStudy = {
  ...defaultStudyInput('product-1', 2, 10, 20),
  id: 'study-1', studyName: 'Test; Premium', status: 'Validated',
  createdAt: '2026-08-01T00:00:00Z', updatedAt: '2026-08-02T00:00:00Z',
  feasibility: { productionScore: 80, marketScore: 75, financialScore: 78, riskScore: 70, globalScore: 77,
    recommendation: 'Viable', strengths: [], weaknesses: [], mainRisks: [], improvements: [], advisedPrice: 20, minimumVolume: 10 },
}

describe('commercial report exports', () => {
  it('génère un CSV complet compatible Excel', () => {
    const csv = buildPortfolioCsv([{ study, productName: 'Biscuit Premium' }])
    expect(csv.charCodeAt(0)).toBe(65279)
    expect(csv).toContain('Test; Premium')
    expect(csv).toContain('77')
  })

  it('échappe le contenu du rapport imprimable', () => {
    const unsafeStudy = { ...study, studyName: '<b>Test</b>' }
    const output = buildExecutiveReportHtml([{ study: unsafeStudy, productName: 'Biscuit' }], new Date('2026-08-12T10:00:00Z'))
    expect(output).toContain('&lt;b&gt;Test&lt;/b&gt;')
    expect(output).not.toContain('<b>Test</b>')
    expect(output).toContain('77/100')
  })
})
