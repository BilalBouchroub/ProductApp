import type { MarketStudy } from '../types/commercial.types'

export interface CommercialReportRow { study: MarketStudy; productName: string }

const csvCell = (value: string | number | null | undefined): string => {
  const quote = String.fromCharCode(34)
  return quote + String(value ?? '').replaceAll(quote, quote + quote) + quote
}
const html = (value: string | number): string => String(value).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
const number = (value: number): string => new Intl.NumberFormat('fr-MA', { maximumFractionDigits: 2 }).format(value)
const money = (value: number): string => new Intl.NumberFormat('fr-MA', { style: 'currency', currency: 'MAD', maximumFractionDigits: 2 }).format(value)
const date = (value: string): string => new Intl.DateTimeFormat('fr-MA', { dateStyle: 'medium' }).format(new Date(value))

export function buildPortfolioCsv(rows: CommercialReportRow[]): string {
  const headers = ['Produit', 'Version', 'Étude', 'Date étude', 'Statut', 'Marché cible', 'Zone géographique', 'Taille du marché (MAD)',
    'Croissance annuelle (%)', 'Coût de production (MAD)', 'Prix proposé (MAD)', 'Prix moyen marché (MAD)', 'Marge (%)',
    'Volume mensuel', 'CA annuel prévu (MAD)', 'Score production', 'Score marché', 'Score financier', 'Maîtrise des risques',
    'Score global', 'Recommandation', 'Concurrents', 'Risques', 'Dernière mise à jour']
  const lines = rows.map(({ study, productName }) => [productName, study.productVersion, study.studyName, study.studyDate, study.status,
    study.targetMarket, study.geographicArea, study.estimatedMarketSize, study.annualGrowthRate, study.pricing.productionCost,
    study.pricing.proposedSalePrice, study.pricing.averageMarketPrice, study.pricing.marginRate, study.forecast.monthlySalesVolume,
    study.forecast.annualRevenue, study.feasibility?.productionScore, study.feasibility?.marketScore, study.feasibility?.financialScore,
    study.feasibility?.riskScore, study.feasibility?.globalScore, study.feasibility?.recommendation, study.competitors.length,
    study.risks.length, study.updatedAt].map(csvCell).join(';'))
  return '\uFEFF' + headers.map(csvCell).join(';') + '\r\n' + lines.join('\r\n')
}

export function buildExecutiveReportHtml(rows: CommercialReportRow[], generatedAt = new Date()): string {
  const validated = rows.filter(row => row.study.status === 'Validated' && row.study.feasibility)
  const viable = validated.filter(row => row.study.feasibility?.recommendation === 'Viable').length
  const averageScore = validated.length ? validated.reduce((sum, row) => sum + row.study.feasibility!.globalScore, 0) / validated.length : 0
  const averageMargin = validated.length ? validated.reduce((sum, row) => sum + row.study.pricing.marginRate, 0) / validated.length : 0
  const annualRevenue = validated.reduce((sum, row) => sum + row.study.forecast.annualRevenue, 0)
  const studyRows = rows.map(({ study, productName }) => '<tr><td><strong>' + html(productName) + '</strong><br><small>' + html(study.studyName) + '</small></td><td>' + html(date(study.studyDate)) + '</td><td>' + html(study.status) + '</td><td>' + (study.feasibility ? html(study.feasibility.globalScore) + '/100' : '—') + '</td><td>' + html(study.feasibility?.recommendation ?? 'En attente') + '</td><td>' + html(number(study.pricing.marginRate)) + ' %</td><td>' + html(money(study.forecast.annualRevenue)) + '</td></tr>').join('')
  return `<!doctype html><html lang=fr><head><meta charset=utf-8><title>Rapport commercial ProductApp</title><style>
  @page{size:A4 landscape;margin:14mm}*{box-sizing:border-box}body{font-family:Arial,sans-serif;color:#0f172a;margin:0;font-size:12px}header{display:flex;justify-content:space-between;align-items:flex-start;border-bottom:3px solid #2563eb;padding-bottom:16px;margin-bottom:20px}h1{font-size:24px;margin:0 0 6px}p{margin:0;color:#64748b}.brand{color:#2563eb;font-weight:800;letter-spacing:.14em;text-transform:uppercase}.kpis{display:grid;grid-template-columns:repeat(5,1fr);gap:10px;margin:18px 0}.kpi{border:1px solid #e2e8f0;border-radius:10px;padding:12px}.kpi span{display:block;color:#64748b;font-size:10px;text-transform:uppercase}.kpi strong{display:block;font-size:18px;margin-top:5px}table{width:100%;border-collapse:collapse;margin-top:16px}th{background:#eff6ff;color:#1d4ed8;text-align:left;font-size:10px;text-transform:uppercase;padding:10px;border-bottom:1px solid #bfdbfe}td{padding:10px;border-bottom:1px solid #e2e8f0;vertical-align:top}small{color:#64748b}footer{margin-top:18px;padding-top:10px;border-top:1px solid #e2e8f0;color:#94a3b8;font-size:10px}@media print{body{-webkit-print-color-adjust:exact;print-color-adjust:exact}}
  </style></head><body><header><div><div class=brand>ProductApp · Intelligence commerciale</div><h1>Rapport exécutif du portefeuille</h1><p>Données issues des études enregistrées sur la plateforme</p></div><div><strong>Généré le ${html(new Intl.DateTimeFormat('fr-MA', { dateStyle: 'long', timeStyle: 'short' }).format(generatedAt))}</strong><p>${rows.length} étude(s) incluse(s)</p></div></header><section class=kpis><div class=kpi><span>Études</span><strong>${rows.length}</strong></div><div class=kpi><span>Validées</span><strong>${validated.length}</strong></div><div class=kpi><span>Viables</span><strong>${viable}</strong></div><div class=kpi><span>Score moyen</span><strong>${number(averageScore)}/100</strong></div><div class=kpi><span>Marge moyenne</span><strong>${number(averageMargin)} %</strong></div></section><p><strong>Chiffre d’affaires annuel prévisionnel consolidé :</strong> ${html(money(annualRevenue))}</p><table><thead><tr><th>Produit / étude</th><th>Date</th><th>Statut</th><th>Score</th><th>Recommandation</th><th>Marge</th><th>CA annuel prévu</th></tr></thead><tbody>${studyRows || '<tr><td colspan=7>Aucune étude dans la sélection.</td></tr>'}</tbody></table><footer>Rapport généré automatiquement par ProductApp. Les projections restent soumises aux hypothèses renseignées dans chaque étude.</footer></body></html>`
}
