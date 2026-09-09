import { BarChart3 } from 'lucide-react'
import type { ReactNode } from 'react'
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { ChartCard } from '../../../../components/charts/ChartCard'
import type { ProductionDashboardData } from '../../types/production.types'

const palette = ['#2563eb', '#8b5cf6', '#10b981', '#f59e0b', '#ef4444', '#06b6d4']
const grid = <CartesianGrid strokeDasharray="4 4" vertical={false} stroke="#e2e8f0" />
const axis = { fontSize: 11, fill: '#64748b' }
const tooltipStyle = { borderRadius: 14, border: '1px solid #e2e8f0', boxShadow: '0 12px 30px rgba(15, 23, 42, .10)' }

function EmptyChart({ label }: { label: string }) {
  return <div className="grid h-full place-items-center rounded-2xl border border-dashed border-slate-200 bg-slate-50/70 px-6 text-center">
    <div><BarChart3 className="mx-auto size-7 text-slate-300" /><p className="mt-2 text-sm font-medium text-slate-500">{label}</p><p className="mt-1 text-xs text-slate-400">Les données apparaîtront automatiquement après saisie.</p></div>
  </div>
}

function DataOrEmpty({ available, label, children }: { available: boolean; label: string; children: ReactNode }) {
  return available ? children : <EmptyChart label={label} />
}

function ComparisonChart({
  title,
  description,
  data,
  unit,
}: {
  title: string
  description: string
  data: { name: string; planned: number; actual: number }[]
  unit: string
}) {
  return <ChartCard title={title} description={description}>
    <DataOrEmpty available={data.length > 0} label={`Aucune comparaison de ${unit.toLowerCase()}`}>
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} margin={{ top: 6, right: 6, left: -12, bottom: 4 }} barGap={5}>
          {grid}<XAxis dataKey="name" tick={axis} tickLine={false} axisLine={false} /><YAxis tick={axis} tickLine={false} axisLine={false} />
          <Tooltip contentStyle={tooltipStyle} cursor={{ fill: '#f8fafc' }} /><Legend iconType="circle" iconSize={8} />
          <Bar dataKey="planned" name={`${unit} prévu`} fill="#cbd5e1" radius={[6, 6, 0, 0]} />
          <Bar dataKey="actual" name={`${unit} réel`} fill="#2563eb" radius={[6, 6, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </DataOrEmpty>
  </ChartCard>
}

export function ProductionDashboardCharts({ data }: { data: ProductionDashboardData }) {
  const statusData = data.productsByStatus.filter(item => item.value > 0)
  const resultData = data.experimentResults.filter(item => item.value > 0)

  return <section aria-label="Indicateurs graphiques de production" className="grid min-w-0 gap-5 xl:grid-cols-2">
    <ChartCard title="Coût planifié par produit" description="Les gammes les plus coûteuses sont affichées en premier.">
      <DataOrEmpty available={data.costByProduct.length > 0} label="Aucun coût de production configuré">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data.costByProduct} margin={{ top: 6, right: 8, left: -8, bottom: 4 }}>
            {grid}<XAxis dataKey="name" tick={axis} tickLine={false} axisLine={false} /><YAxis tick={axis} tickLine={false} axisLine={false} />
            <Tooltip contentStyle={tooltipStyle} cursor={{ fill: '#eff6ff' }} />
            <Bar dataKey="cost" name="Coût planifié" fill="#2563eb" radius={[8, 8, 2, 2]} maxBarSize={48} />
          </BarChart>
        </ResponsiveContainer>
      </DataOrEmpty>
    </ChartCard>

    <ChartCard title="Durée des chaînes" description="Temps total prévu pour chaque gamme de production.">
      <DataOrEmpty available={data.durationByProduct.length > 0} label="Aucune durée de chaîne configurée">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data.durationByProduct} margin={{ top: 8, right: 8, left: -8, bottom: 4 }}>
            <defs><linearGradient id="durationGradient" x1="0" y1="0" x2="0" y2="1"><stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.35} /><stop offset="95%" stopColor="#8b5cf6" stopOpacity={0.02} /></linearGradient></defs>
            {grid}<XAxis dataKey="name" tick={axis} tickLine={false} axisLine={false} /><YAxis tick={axis} tickLine={false} axisLine={false} />
            <Tooltip contentStyle={tooltipStyle} /><Area type="monotone" dataKey="duration" name="Durée (min)" stroke="#8b5cf6" strokeWidth={3} fill="url(#durationGradient)" />
          </AreaChart>
        </ResponsiveContainer>
      </DataOrEmpty>
    </ChartCard>

    <ChartCard title="Portefeuille par statut" description="Répartition actuelle des produits dans le workflow.">
      <DataOrEmpty available={statusData.length > 0} label="Aucun produit dans le portefeuille">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart><Pie data={statusData} dataKey="value" nameKey="name" innerRadius={62} outerRadius={92} paddingAngle={3} stroke="none">{statusData.map((item, index) => <Cell key={item.name} fill={palette[index % palette.length]} />)}</Pie><Tooltip contentStyle={tooltipStyle} /><Legend iconType="circle" iconSize={8} /></PieChart>
        </ResponsiveContainer>
      </DataOrEmpty>
    </ChartCard>

    <ChartCard title="Résultats des expériences" description="Lecture rapide de la performance des essais.">
      <DataOrEmpty available={resultData.length > 0} label="Aucun résultat d’expérience disponible">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={resultData} layout="vertical" margin={{ top: 4, right: 12, left: 6, bottom: 4 }}>
            {grid}<XAxis type="number" allowDecimals={false} tick={axis} tickLine={false} axisLine={false} /><YAxis dataKey="name" type="category" width={82} tick={axis} tickLine={false} axisLine={false} />
            <Tooltip contentStyle={tooltipStyle} cursor={{ fill: '#f8fafc' }} /><Bar dataKey="value" name="Expériences" radius={[0, 8, 8, 0]} maxBarSize={30}>{resultData.map((item, index) => <Cell key={item.name} fill={palette[(index + 2) % palette.length]} />)}</Bar>
          </BarChart>
        </ResponsiveContainer>
      </DataOrEmpty>
    </ChartCard>

    <ComparisonChart title="Coûts : prévu et réel" description="Écarts budgétaires des derniers essais." data={data.costComparison} unit="Coût" />
    <ComparisonChart title="Durées : prévu et réel" description="Écarts de temps des derniers essais." data={data.durationComparison} unit="Temps" />

    <ChartCard title="Consommation des ressources" description="Quantité planifiée comparée à la consommation réelle.">
      <DataOrEmpty available={data.resourceConsumption.length > 0} label="Aucune ressource renseignée">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data.resourceConsumption} margin={{ top: 6, right: 6, left: -12, bottom: 4 }}>
            {grid}<XAxis dataKey="name" tick={axis} tickLine={false} axisLine={false} /><YAxis tick={axis} tickLine={false} axisLine={false} />
            <Tooltip contentStyle={tooltipStyle} cursor={{ fill: '#f8fafc' }} /><Legend iconType="circle" iconSize={8} />
            <Bar dataKey="planned" name="Planifiée" fill="#bae6fd" radius={[6, 6, 0, 0]} /><Bar dataKey="actual" name="Réelle" fill="#06b6d4" radius={[6, 6, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </DataOrEmpty>
    </ChartCard>

    <ChartCard title="Coût par étape" description="Étapes qui concentrent le plus de coûts planifiés.">
      <DataOrEmpty available={data.costByStep.length > 0} label="Aucun coût d’étape disponible">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data.costByStep} layout="vertical" margin={{ top: 4, right: 12, left: 16, bottom: 4 }}>
            {grid}<XAxis type="number" tick={axis} tickLine={false} axisLine={false} /><YAxis dataKey="name" type="category" width={92} tick={axis} tickLine={false} axisLine={false} />
            <Tooltip contentStyle={tooltipStyle} cursor={{ fill: '#fff7ed' }} /><Bar dataKey="cost" name="Coût" fill="#f59e0b" radius={[0, 8, 8, 0]} maxBarSize={28} />
          </BarChart>
        </ResponsiveContainer>
      </DataOrEmpty>
    </ChartCard>
  </section>
}
