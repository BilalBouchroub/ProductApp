import {
  ArrowRight,
  BarChart3,
  Beaker,
  Bot,
  Boxes,
  BrainCircuit,
  Check,
  ChevronRight,
  CircleDollarSign,
  Database,
  Factory,
  FileText,
  FlaskConical,
  Gauge,
  History,
  Layers3,
  LockKeyhole,
  Menu,
  MessageSquareText,
  PackageSearch,
  Server,
  ShieldCheck,
  Sparkles,
  TrendingUp,
  UserRound,
  Users,
  Workflow,
  X,
  Zap,
  type LucideIcon,
} from 'lucide-react'
import { useEffect, useRef, useState, type CSSProperties, type PointerEvent as ReactPointerEvent, type ReactNode } from 'react'
import { useAuth } from '../../../hooks/useAuth'
import { getRoleHome } from '../../../utils/roleRedirect'
import LandingSmartSection from '../components/LandingSmartSection'
import './landing.css'

const navItems = [
  ['Produit', '#produit'],
  ['Fonctionnalités', '#fonctionnalites'],
  ['SMART PRODUCT', '#smart-product'],
  ['Sécurité', '#securite'],
  ['À propos', '#apropos'],
] as const

const capabilities: ReadonlyArray<{ icon: LucideIcon; title: string; text: string; accent: string }> = [
  { icon: PackageSearch, title: 'Référentiel produit', text: 'Produits, catégories, versions et fiches techniques réunis dans un espace cohérent.', accent: 'blue' },
  { icon: Workflow, title: 'Chaînes de production', text: 'Étapes, durées, dépendances et avancement visibles de bout en bout.', accent: 'cyan' },
  { icon: Factory, title: 'Ressources & SAP', text: 'Matières, machines et équipements associés au bon contexte métier.', accent: 'violet' },
  { icon: CircleDollarSign, title: 'Coûts & marges', text: 'Coût de revient, consommation, marge estimée et pistes d’optimisation.', accent: 'pink' },
  { icon: FlaskConical, title: 'Expériences', text: 'Paramètres, résultats et comparaisons pour capitaliser sur chaque essai.', accent: 'violet' },
  { icon: TrendingUp, title: 'Décision commerciale', text: 'Études de marché, faisabilité, concurrence, projections et rapports.', accent: 'blue' },
  { icon: FileText, title: 'Documents & rapports', text: 'Pièces utiles, synthèses structurées et exports pour partager les décisions.', accent: 'cyan' },
  { icon: ShieldCheck, title: 'Utilisateurs & audit', text: 'Rôles, permissions, traçabilité et isolation des données par utilisateur.', accent: 'pink' },
]

const businessAreas = [
  {
    eyebrow: '01 · Production', title: 'Du processus à la performance réelle.',
    text: 'Construisez les chaînes de production, reliez les ressources et identifiez rapidement les coûts, durées ou anomalies qui méritent une action.',
    bullets: ['Processus et étapes ordonnés', 'Matières, machines et ressources', 'Coûts, durées et consommation', 'Anomalies et optimisation'],
    icon: Factory, tone: 'blue', visual: 'production',
  },
  {
    eyebrow: '02 · Expériences', title: 'Chaque essai devient une connaissance utile.',
    text: 'Créez, suivez et comparez les expériences sans perdre le lien avec le produit, ses paramètres et ses résultats observés.',
    bullets: ['Paramètres et protocoles', 'Résultats planifiés et réels', 'Comparaison multicritère', 'Recommandations documentées'],
    icon: Beaker, tone: 'violet', visual: 'experiment',
  },
  {
    eyebrow: '03 · Commerce', title: 'La réalité industrielle éclaire le marché.',
    text: 'Croisez les contraintes de production avec les prix, la concurrence et les études de marché pour mieux estimer la faisabilité.',
    bullets: ['Données commerciales structurées', 'Prix, concurrence et projections', 'Score de faisabilité', 'Rapports décisionnels'],
    icon: BarChart3, tone: 'cyan', visual: 'commercial',
  },
  {
    eyebrow: '04 · Administration', title: 'Le contrôle sans ralentir les équipes.',
    text: 'Administrez les accès, suivez l’activité et conservez un historique clair dans une plateforme organisée par responsabilités.',
    bullets: ['Utilisateurs et rôles', 'Permissions par espace', 'Historique et audit', 'Isolation des données'],
    icon: ShieldCheck, tone: 'pink', visual: 'admin',
  },
] as const

const securityItems = [
  ['Authentification requise', LockKeyhole], ['Permissions ProductApp respectées', ShieldCheck],
  ['Historique séparé par utilisateur', UserRound], ['Clé Gemini conservée côté backend', Server],
  ['Aucun accès SQL libre par l’IA', Database], ['Outils métier en lecture seule', Check],
  ['Documents traités comme non fiables', FileText], ['Instructions malveillantes neutralisées', ShieldCheck],
  ['Réponse honnête si la donnée manque', MessageSquareText],
] as const

function useReducedMotion() {
  const [reduced, setReduced] = useState(false)
  useEffect(() => {
    const media = window.matchMedia('(prefers-reduced-motion: reduce)')
    const update = () => setReduced(media.matches)
    update(); media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [])
  return reduced
}

function useReveal() {
  useEffect(() => {
    const nodes = Array.from(document.querySelectorAll<HTMLElement>('.landing-page [data-reveal]'))
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      nodes.forEach(node => node.classList.add('is-visible')); return
    }
    const observer = new IntersectionObserver(entries => {
      entries.forEach(entry => { if (entry.isIntersecting) { entry.target.classList.add('is-visible'); observer.unobserve(entry.target) } })
    }, { threshold: 0.12, rootMargin: '0px 0px -50px' })
    nodes.forEach(node => observer.observe(node))
    return () => observer.disconnect()
  }, [])
}

function MagneticLink({ to, children, variant = 'primary', className = '' }: { to: string; children: ReactNode; variant?: 'primary' | 'secondary' | 'dark'; className?: string }) {
  const ref = useRef<HTMLAnchorElement>(null)
  const onMove = (event: ReactPointerEvent<HTMLAnchorElement>) => {
    if (event.pointerType === 'touch' || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const rect = event.currentTarget.getBoundingClientRect()
    const x = (event.clientX - rect.left - rect.width / 2) * 0.12
    const y = (event.clientY - rect.top - rect.height / 2) * 0.12
    event.currentTarget.style.transform = `translate3d(${x}px, ${y}px, 0)`
  }
  const reset = () => { if (ref.current) ref.current.style.transform = '' }
  return <a ref={ref} href={to} onPointerMove={onMove} onPointerLeave={reset} className={`lp-button lp-button--${variant} ${className}`}>{children}</a>
}

function Brand({ light = false }: { light?: boolean }) {
  return <span className={`lp-brand ${light ? 'lp-brand--light' : ''}`}><span className="lp-brand__mark"><Boxes aria-hidden="true" /></span><span><strong>ProductApp</strong><small>Operations Intelligence</small></span></span>
}

function CountUp({ value, suffix = '' }: { value: number; suffix?: string }) {
  const ref = useRef<HTMLSpanElement>(null)
  const [count, setCount] = useState(0)
  useEffect(() => {
    const node = ref.current; if (!node) return
    const observer = new IntersectionObserver(([entry]) => {
      if (!entry?.isIntersecting) return
      if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) { setCount(value); observer.disconnect(); return }
      const start = performance.now(); const duration = 900
      const tick = (time: number) => { const progress = Math.min(1, (time - start) / duration); setCount(Math.round(value * (1 - Math.pow(1 - progress, 3)))); if (progress < 1) requestAnimationFrame(tick) }
      requestAnimationFrame(tick); observer.disconnect()
    }, { threshold: .6 })
    observer.observe(node); return () => observer.disconnect()
  }, [value])
  return <span ref={ref}>{count}{suffix}</span>
}

function Header({ dashboardPath }: { dashboardPath: string }) {
  const [scrolled, setScrolled] = useState(false)
  const [open, setOpen] = useState(false)
  useEffect(() => { const update = () => setScrolled(window.scrollY > 24); update(); window.addEventListener('scroll', update, { passive: true }); return () => window.removeEventListener('scroll', update) }, [])
  useEffect(() => { if (!open) return; const close = (event: KeyboardEvent) => { if (event.key === 'Escape') setOpen(false) }; window.addEventListener('keydown', close); return () => window.removeEventListener('keydown', close) }, [open])
  return <header className={`lp-header ${scrolled ? 'is-scrolled' : ''}`}>
    <div className="lp-container lp-header__inner">
      <a href="#top" className="lp-logo-link" aria-label="ProductApp, retour en haut"><Brand /></a>
      <nav className="lp-nav" aria-label="Navigation de la landing page">{navItems.map(([label, href]) => <a key={href} href={href}>{label}</a>)}</nav>
      <div className="lp-header__actions"><a className="lp-login-link" href="/login">Se connecter</a><MagneticLink to={dashboardPath}>Découvrir ProductApp <ArrowRight aria-hidden="true" /></MagneticLink></div>
      <button type="button" className="lp-menu-button" aria-label={open ? 'Fermer le menu' : 'Ouvrir le menu'} aria-expanded={open} aria-controls="mobile-menu" onClick={() => setOpen(value => !value)}>{open ? <X /> : <Menu />}</button>
    </div>
    <div id="mobile-menu" className={`lp-mobile-menu ${open ? 'is-open' : ''}`} aria-hidden={!open} inert={!open}><nav aria-label="Navigation mobile">{navItems.map(([label, href]) => <a key={href} href={href} onClick={() => setOpen(false)}>{label}<ChevronRight /></a>)}<a href="/login" onClick={() => setOpen(false)}>Se connecter<ChevronRight /></a><MagneticLink to={dashboardPath}>Découvrir ProductApp <ArrowRight /></MagneticLink></nav></div>
  </header>
}

function HeroDemo() {
  const demoRef = useRef<HTMLDivElement>(null)
  const tilt = (event: ReactPointerEvent<HTMLDivElement>) => {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches || window.innerWidth < 980) return
    const rect = event.currentTarget.getBoundingClientRect(); const x = (event.clientX - rect.left) / rect.width - .5; const y = (event.clientY - rect.top) / rect.height - .5
    event.currentTarget.style.setProperty('--tilt-x', `${-y * 2.4}deg`); event.currentTarget.style.setProperty('--tilt-y', `${x * 3.5}deg`)
  }
  const reset = () => { demoRef.current?.style.setProperty('--tilt-x', '0deg'); demoRef.current?.style.setProperty('--tilt-y', '0deg') }
  return <div className="lp-hero-demo-wrap" aria-label="Démonstration visuelle de ProductApp et SMART PRODUCT">
    <div ref={demoRef} className="lp-hero-demo" onPointerMove={tilt} onPointerLeave={reset}>
      <div className="lp-demo-topbar"><Brand /><span className="lp-demo-status"><i /> Démonstration visuelle</span><span className="lp-demo-user">PA</span></div>
      <div className="lp-demo-layout">
        <aside className="lp-demo-sidebar" aria-hidden="true"><span className="active"><Gauge /></span><span><PackageSearch /></span><span><Workflow /></span><span><FlaskConical /></span><span><BarChart3 /></span></aside>
        <div className="lp-demo-dashboard">
          <div className="lp-demo-heading"><div><small>Produit sélectionné</small><strong>P001 · Biscuit Démo</strong></div><span>Prototype visuel</span></div>
          <div className="lp-kpi-row"><div><small>Coût total</small><strong>128,40 DH</strong><em>calcul simulé</em></div><div><small>Temps total</small><strong>2 h 45</strong><em>4 étapes</em></div><div><small>Étape critique</small><strong>Cuisson</strong><em>42 min</em></div></div>
          <div className="lp-demo-main-card"><div className="lp-card-title"><span>Coût par étape</span><small>Données de démonstration</small></div><div className="lp-bars" aria-hidden="true"><span style={{ height: '32%' }} /><span style={{ height: '51%' }} /><span className="hot" style={{ height: '86%' }} /><span style={{ height: '46%' }} /><span style={{ height: '64%' }} /></div><div className="lp-chart-labels"><span>Prépa.</span><span>Mélange</span><span>Cuisson</span><span>Repos</span><span>Emballage</span></div></div>
          <div className="lp-step-flow"><span className="done">01<small>Préparation</small></span><i /><span className="done">02<small>Mélange</small></span><i /><span className="critical">03<small>Cuisson</small></span><i /><span>04<small>Emballage</small></span></div>
        </div>
        <div className="lp-demo-chat"><div className="lp-chat-head"><span><Sparkles /> SMART PRODUCT</span><i>Connecté</i></div><div className="lp-chat-content"><div className="lp-chat-user">Analyse profondément le produit P001.</div><div className="lp-chat-ai"><span className="lp-ai-mark"><BrainCircuit /></span><div><p>Le produit présente un coût total simulé de <strong>128,40 DH</strong>.</p><ul><li><b>Étape critique :</b> cuisson</li><li><b>Ressource dominante :</b> farine</li><li><b>Recommandation :</b> vérifier le réglage thermique avant le prochain essai.</li></ul><div className="lp-source"><Database /> 3 références ProductApp utilisées</div></div></div></div><div className="lp-stream-line"><span /><span /><span /></div></div>
      </div>
    </div>
    <div className="lp-float-card lp-float-card--one"><Zap /><span><small>Analyse contextualisée</small><strong>Sources internes</strong></span></div>
    <div className="lp-float-card lp-float-card--two"><ShieldCheck /><span><small>Accès contrôlé</small><strong>Lecture seule</strong></span></div>
  </div>
}

function CapabilityCard({ item, index }: { item: typeof capabilities[number]; index: number }) {
  const Icon = item.icon
  return <article className={`lp-cap-card lp-tone-${item.accent}`} data-reveal style={{ '--delay': `${(index % 4) * 70}ms` } as CSSProperties}><span className="lp-cap-icon"><Icon /></span><h3>{item.title}</h3><p>{item.text}</p><span className="lp-cap-arrow" aria-hidden="true"><ArrowRight /></span></article>
}

function Architecture() {
  const nodes = [['Utilisateur', UserRound], ['SMART PRODUCT', Sparkles], ['Backend ProductApp', Server], ['Outils autorisés', ShieldCheck], ['Services métier', Workflow], ['Base de données', Database]] as const
  return <section className="lp-section lp-architecture" aria-labelledby="architecture-title">
    <div className="lp-container"><div className="lp-section-heading lp-section-heading--center" data-reveal><span className="lp-eyebrow"><Layers3 /> Architecture intelligente</span><h2 id="architecture-title">L’intelligence reste encadrée<br />par votre logique métier.</h2><p>Le modèle ne contourne jamais ProductApp. Les accès, filtres, calculs et permissions restent appliqués côté serveur.</p></div>
      <div className="lp-arch-panel" data-reveal><div className="lp-arch-flow">{nodes.map(([label, Icon], index) => <div className="lp-arch-step" key={label}><div className={label === 'SMART PRODUCT' ? 'accent' : ''}><Icon /><span>{label}</span></div>{index < nodes.length - 1 && <span className="lp-flow-line" aria-hidden="true"><i /></span>}</div>)}</div><div className="lp-arch-ai"><span className="lp-flow-line lp-flow-line--vertical" aria-hidden="true"><i /></span><div><Bot /><span><small>Fournisseur IA</small><strong>Gemini</strong></span></div><p>Reçoit uniquement le contexte utile et retourne une réponse contextualisée.</p></div></div>
      <div className="lp-architecture-points" data-reveal>{['Aucune connexion SQL directe', 'Calculs déterministes côté serveur', 'Données filtrées par permission', 'Sources internes citées', 'Conversations persistantes'].map(item => <span key={item}><Check />{item}</span>)}</div>
    </div>
  </section>
}

function BusinessVisual({ type }: { type: string }) {
  if (type === 'production') return <div className="lp-business-visual lp-visual-production"><div className="lp-visual-head"><span>Chaîne de production</span><small>Produit P001 · Démo</small></div><div className="lp-process-list">{[['01', 'Préparation', '18 min'], ['02', 'Mélange', '31 min'], ['03', 'Cuisson', '42 min'], ['04', 'Emballage', '24 min']].map(([n, label, time], index) => <div key={label} className={index === 2 ? 'critical' : ''}><b>{n}</b><span><strong>{label}</strong><small>{index === 2 ? 'Étape à surveiller' : 'Dans la plage prévue'}</small></span><em>{time}</em></div>)}</div></div>
  if (type === 'experiment') return <div className="lp-business-visual lp-visual-experiment"><div className="lp-visual-head"><span>Comparaison d’expériences</span><small>Données de démonstration</small></div><div className="lp-experiment-head"><span>EXP-21</span><span>EXP-25</span></div>{[['Rendement', 74, 88], ['Temps', 82, 65], ['Coût', 62, 56], ['Qualité', 78, 91]].map(([label, a, b]) => <div className="lp-compare-row" key={String(label)}><span>{label}</span><div><i style={{ width: `${a}%` }} /></div><div><i style={{ width: `${b}%` }} /></div></div>)}<div className="lp-recommendation"><Sparkles /><span><small>Recommandation simulée</small><strong>EXP-25 offre le meilleur équilibre.</strong></span></div></div>
  if (type === 'commercial') return <div className="lp-business-visual lp-visual-commercial"><div className="lp-visual-head"><span>Faisabilité commerciale</span><small>Scénario démo</small></div><div className="lp-score-ring"><span><strong>À étudier</strong><small>Potentiel favorable</small></span></div><div className="lp-mini-kpis"><span><small>Marge estimée</small><strong>Positive</strong></span><span><small>Concurrence</small><strong>Modérée</strong></span><span><small>Risque</small><strong>À surveiller</strong></span></div></div>
  return <div className="lp-business-visual lp-visual-admin"><div className="lp-visual-head"><span>Contrôle des accès</span><small>Rôles ProductApp</small></div>{[['AM', 'Administrator', 'Tous les contrôles'], ['PM', 'ProductionManager', 'Espace production'], ['CM', 'CommercialManager', 'Espace commercial']].map(([initials, role, access], index) => <div className="lp-role-row" key={role}><b className={`role-${index}`}>{initials}</b><span><strong>{role}</strong><small>{access}</small></span><em><ShieldCheck /> Autorisé</em></div>)}<div className="lp-audit-line"><History /><span><strong>Traçabilité active</strong><small>Actions sensibles historisées</small></span></div></div>
}

function FeatureFormats() {
  return <section className="lp-section lp-formats" aria-labelledby="formats-title"><div className="lp-container lp-format-layout"><div className="lp-section-heading" data-reveal><span className="lp-eyebrow"><Sparkles /> Réponses enrichies</span><h2 id="formats-title">Une réponse qui prend la bonne forme.</h2><p>SMART PRODUCT organise les résultats pour rendre l’analyse immédiatement lisible et exploitable.</p><div className="lp-format-tags">{['Markdown', 'Tableaux', 'KPI', 'Graphiques', 'Alertes', 'Recommandations', 'Sources', 'Documents'].map(label => <span key={label}>{label}</span>)}</div></div><div className="lp-format-canvas" data-reveal><div className="lp-format-card lp-format-card--summary"><small>Synthèse</small><h3>La cuisson concentre le principal levier d’optimisation.</h3><p>Une vérification du réglage thermique est recommandée avant le prochain essai.</p></div><div className="lp-format-card lp-format-card--kpi"><small>Coût estimé</small><strong>128,40 <em>DH</em></strong><span>Démo visuelle</span></div><div className="lp-format-card lp-format-card--chart"><small>Consommation</small><div><i style={{ height: '42%' }} /><i style={{ height: '66%' }} /><i style={{ height: '88%' }} /><i style={{ height: '54%' }} /></div></div><div className="lp-format-card lp-format-card--source"><Database /><span><small>Sources internes</small><strong>Produit · Processus · EXP-25</strong></span></div><div className="lp-format-card lp-format-card--alert"><Zap /><span><small>Point d’attention</small><strong>Temps de cuisson élevé</strong></span></div></div></div></section>
}

function Security() {
  return <section id="securite" className="lp-section lp-security" aria-labelledby="security-title"><div className="lp-security-orb" aria-hidden="true" /><div className="lp-container lp-security-layout"><div className="lp-section-heading" data-reveal><span className="lp-eyebrow lp-eyebrow--dark"><ShieldCheck /> Sécurité & confidentialité</span><h2 id="security-title">Vos données restent sous le contrôle de ProductApp.</h2><p>SMART PRODUCT s’appuie sur l’architecture applicative existante. Il utilise des outils métier explicitement autorisés, sans accès libre aux données.</p><div className="lp-security-badge"><LockKeyhole /><span><strong>IA encadrée</strong><small>Permissions, validation et traçabilité côté backend</small></span></div></div><div className="lp-security-grid" data-reveal>{securityItems.map(([label, Icon]) => <div key={label}><span><Icon /></span><p>{label}</p></div>)}</div></div></section>
}

function Spaces() {
  return <section className="lp-section lp-spaces" aria-labelledby="spaces-title"><div className="lp-container"><div className="lp-section-heading lp-section-heading--center" data-reveal><span className="lp-eyebrow"><Users /> Expérience multi-espace</span><h2 id="spaces-title">Une intelligence commune.<br />Un contexte propre à chacun.</h2><p>SMART PRODUCT accompagne chaque responsabilité depuis son espace autorisé, tout en conservant un historique personnel et isolé.</p></div><div className="lp-space-grid"><article data-reveal><span className="admin"><ShieldCheck /></span><h3>Administration</h3><p>Utilisateurs, rôles, permissions, audit et gouvernance.</p><small><i /> Historique personnel</small></article><article data-reveal><span className="production"><Factory /></span><h3>Production</h3><p>Produits, processus, ressources, coûts et expériences.</p><small><i /> Historique personnel</small></article><article data-reveal><span className="commercial"><BarChart3 /></span><h3>Commercial / Marketing</h3><p>Marché, prix, faisabilité, projections et rapports.</p><small><i /> Historique personnel</small></article></div></div></section>
}

export function LandingPage() {
  const { session } = useAuth()
  const reducedMotion = useReducedMotion()
  const rootRef = useRef<HTMLElement>(null)
  useReveal()
  const dashboardPath = session ? getRoleHome(session.user.role) : '/login'
  const moveGlow = (event: ReactPointerEvent<HTMLElement>) => {
    if (reducedMotion || window.innerWidth < 768) return
    event.currentTarget.style.setProperty('--mouse-x', `${event.clientX}px`)
    event.currentTarget.style.setProperty('--mouse-y', `${event.clientY}px`)
    event.currentTarget.style.setProperty('--parallax-x', `${(event.clientX / window.innerWidth - .5) * 14}px`)
    event.currentTarget.style.setProperty('--parallax-y', `${(event.clientY / window.innerHeight - .5) * 10}px`)
  }

  return <main ref={rootRef} id="top" className="landing-page" onPointerMove={moveGlow}>
    <a className="lp-skip-link" href="#main-content-landing">Aller au contenu</a>
    <Header dashboardPath={dashboardPath} />
    <div className="lp-cursor-glow" aria-hidden="true" />
    <section id="main-content-landing" className="lp-hero" aria-labelledby="hero-title">
      <div className="lp-hero-bg" aria-hidden="true"><span className="orb orb-one" /><span className="orb orb-two" /><span className="orb orb-three" /><span className="lp-grid-plane" />{Array.from({ length: 12 }, (_, index) => <i key={index} style={{ '--i': index } as CSSProperties} />)}</div>
      <div className="lp-container lp-hero__content"><div className="lp-hero-copy"><div className="lp-hero-badge"><Sparkles /> Product Intelligence Platform <span>Nouveau</span></div><h1 id="hero-title">Pilotez vos produits<br />avec <span>intelligence.</span></h1><p>ProductApp centralise vos produits, processus de production, expériences, ressources, coûts et analyses commerciales dans une plateforme métier intelligente.</p><div className="lp-ai-promise"><span><BrainCircuit /></span><p><strong>SMART PRODUCT</strong> — votre assistant IA intégré aux données réelles de ProductApp.</p></div><div className="lp-hero-actions"><MagneticLink to={dashboardPath}>Découvrir la plateforme <ArrowRight /></MagneticLink><a className="lp-button lp-button--secondary" href="#smart-product"><Sparkles /> Explorer SMART PRODUCT</a></div><div className="lp-hero-trust"><span><ShieldCheck /> Accès contrôlé</span><span><Database /> Sources internes</span><span><Zap /> Réponses contextualisées</span></div></div><HeroDemo /></div>
    </section>

    <section className="lp-signal-strip" aria-label="Indicateurs clés de ProductApp"><div className="lp-container"><div><strong><CountUp value={1} /></strong><span>Plateforme unifiée<small>Données métier centralisées</small></span></div><div><strong><CountUp value={3} /></strong><span>Espaces spécialisés<small>Administration · Production · Commercial</small></span></div><div><strong><CountUp value={8} /></strong><span>Formats enrichis<small>Texte · KPI · tableaux · graphiques</small></span></div><div><span className="lp-live-dot"><i /></span><span>Analyse contextualisée<small>Historique persistant par utilisateur</small></span></div></div></section>

    <section id="produit" className="lp-section lp-product" aria-labelledby="product-title"><div className="lp-container"><div className="lp-section-heading lp-section-heading--split" data-reveal><div><span className="lp-eyebrow"><Boxes /> La plateforme ProductApp</span><h2 id="product-title">Un seul fil conducteur,<br />du produit à la décision.</h2></div><p>ProductApp relie les informations industrielles et commerciales qui vivent trop souvent dans des outils séparés. Chaque équipe travaille dans son espace, avec la même source de vérité.</p></div><div className="lp-cap-grid">{capabilities.map((item, index) => <CapabilityCard item={item} index={index} key={item.title} />)}</div></div></section>

    <div id="smart-product" className="lp-lazy-anchor"><LandingSmartSection /></div>

    <Architecture />

    <section id="fonctionnalites" className="lp-section lp-business" aria-labelledby="business-title"><div className="lp-container"><div className="lp-section-heading lp-section-heading--center" data-reveal><span className="lp-eyebrow"><Workflow /> Fonctionnalités métier</span><h2 id="business-title">Chaque équipe voit l’essentiel.<br />Toute l’entreprise avance ensemble.</h2></div><div className="lp-business-list">{businessAreas.map((area, index) => { const Icon = area.icon; return <article className={`lp-business-row lp-business-row--${area.tone}`} key={area.title} data-reveal><div className="lp-business-copy"><span className="lp-business-icon"><Icon /></span><small>{area.eyebrow}</small><h3>{area.title}</h3><p>{area.text}</p><ul>{area.bullets.map(item => <li key={item}><Check />{item}</li>)}</ul></div><BusinessVisual type={area.visual} /><span className="lp-row-number" aria-hidden="true">0{index + 1}</span></article> })}</div></div></section>

    <FeatureFormats />
    <Security />
    <Spaces />

    <section className="lp-final-cta" aria-labelledby="cta-title"><div className="lp-cta-bg" aria-hidden="true"><span /><span /><i /></div><div className="lp-container" data-reveal><span className="lp-cta-icon"><Sparkles /></span><h2 id="cta-title">Transformez vos données produit<br />en décisions concrètes.</h2><p>Centralisez vos opérations, analysez vos performances et dialoguez avec vos données grâce à SMART PRODUCT.</p><div><MagneticLink to={dashboardPath} variant="primary">Accéder à ProductApp <ArrowRight /></MagneticLink><MagneticLink to="/ai" variant="dark"><Sparkles /> Découvrir SMART PRODUCT</MagneticLink></div><small><ShieldCheck /> Une expérience adaptée à votre rôle et à vos permissions.</small></div></section>

    <footer id="apropos" className="lp-footer"><div className="lp-container"><div className="lp-footer-main"><div><Brand light /><p>La plateforme métier qui relie produits, production, expériences, coûts, commerce et intelligence artificielle.</p></div><div><h2>Plateforme</h2><a href="#produit">Produit</a><a href="#fonctionnalites">Production</a><a href="#fonctionnalites">Expériences</a><a href="#fonctionnalites">Commercial</a></div><div><h2>Intelligence</h2><a href="#smart-product">SMART PRODUCT</a><a href="#securite">Sécurité</a><a href="/ai">Ouvrir l’assistant</a></div><div><h2>Accès</h2><a href="/login">Connexion</a><a href={dashboardPath}>Tableau de bord</a><a href="#top">Retour en haut</a></div></div><div className="lp-footer-bottom"><span>© {new Date().getFullYear()} ProductApp. Tous droits réservés.</span><span>Plateforme de démonstration · Données visuelles signalées</span></div></div></footer>
  </main>
}
