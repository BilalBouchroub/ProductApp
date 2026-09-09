import { ArrowUpRight, BarChart3, BrainCircuit, Check, Database, FileText, PackageSearch, Search, Sparkles, Zap } from 'lucide-react'
import { useEffect, useState } from 'react'

const questions = [
  'Affiche les produits existants.',
  'Analyse profondément le produit P001.',
  'Quelle étape coûte le plus cher ?',
  'Quelle ressource est la plus consommée ?',
  'Compare les expériences EXP-21 et EXP-25.',
  'Comment réduire les coûts de production ?',
  'Détecte les anomalies.',
  'Résume ce document.',
  'Génère une synthèse commerciale.',
  'Prépare un rapport de production.',
] as const

const responseLead = [
  'Voici les produits de démonstration disponibles dans cet aperçu.',
  'L’analyse simulée de P001 met en évidence la cuisson comme étape prioritaire.',
  'Dans ce scénario visuel, la cuisson présente le coût le plus élevé.',
  'La farine est la ressource la plus consommée dans les données de démonstration.',
  'EXP-25 présente ici un meilleur équilibre entre rendement, temps et coût.',
  'Trois leviers se dégagent : réglage thermique, dosage et temps de cycle.',
  'Une variation de durée est détectée sur l’étape de cuisson.',
  'Le document décrit un essai de production et ses principaux résultats.',
  'Le potentiel commercial est favorable sous réserve de maîtriser le coût cible.',
  'Le rapport rassemble les KPI, écarts, risques et recommandations utiles.',
] as const

const abilities = [
  'Récupère les produits autorisés', 'Analyse les étapes et ressources', 'Interprète coûts et durées',
  'Compare les expériences', 'Recherche dans les documents', 'Cite les sources internes',
  'Conserve le fil de la conversation', 'Propose des recommandations',
]

export default function LandingSmartSection() {
  const [active, setActive] = useState(1)
  const [displayed, setDisplayed] = useState('')
  const fullResponse = responseLead[active] ?? responseLead[0]

  useEffect(() => {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) { setDisplayed(fullResponse); return }
    setDisplayed('')
    let index = 0
    const timer = window.setInterval(() => {
      index += 2
      setDisplayed(fullResponse.slice(0, index))
      if (index >= fullResponse.length) window.clearInterval(timer)
    }, 24)
    return () => window.clearInterval(timer)
  }, [fullResponse])

  const selectQuestion = (index: number) => {
    setActive(index)
    document.getElementById('smart-demo')?.scrollIntoView({ behavior: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth', block: 'center' })
  }

  return <>
    <section className="lp-section lp-smart" aria-labelledby="smart-title">
      <div className="lp-smart-glow" aria-hidden="true" />
      <div className="lp-container lp-smart-layout">
        <div className="lp-smart-copy" data-reveal>
          <span className="lp-eyebrow lp-eyebrow--dark"><Sparkles /> SMART PRODUCT</span>
          <h2 id="smart-title">Une IA qui comprend<br />réellement votre activité.</h2>
          <p>Ce n’est pas un chatbot générique. SMART PRODUCT dialogue avec les services ProductApp autorisés pour transformer vos données métier en analyses structurées, traçables et utiles.</p>
          <div className="lp-ability-grid">{abilities.map(item => <span key={item}><Check />{item}</span>)}</div>
          <a href="#questions" className="lp-text-link">Voir les questions possibles <ArrowUpRight /></a>
        </div>

        <div id="smart-demo" className="lp-smart-chat" data-reveal>
          <div className="lp-smart-chat__top"><span><BrainCircuit /> SMART PRODUCT</span><div><i /> Données contrôlées</div></div>
          <div className="lp-smart-chat__body">
            <div className="lp-demo-notice"><Zap /> Démonstration visuelle — aucune requête API n’est effectuée.</div>
            <div className="lp-smart-message lp-smart-message--user">{questions[active]}</div>
            <div className="lp-smart-message lp-smart-message--ai"><span><Sparkles /></span><div><p>{displayed}<i className={displayed.length < fullResponse.length ? 'lp-caret' : ''} /></p>{active === 0 ? <ProductTable /> : <AnalysisResult active={active} />}</div></div>
          </div>
          <div className="lp-smart-composer"><span><Search /> Poser une question sur vos produits…</span><button type="button" aria-label="Envoyer la question simulée"><ArrowUpRight /></button></div>
        </div>
      </div>
    </section>

    <section id="questions" className="lp-section lp-questions" aria-labelledby="questions-title">
      <div className="lp-container"><div className="lp-section-heading lp-section-heading--center" data-reveal><span className="lp-eyebrow"><BrainCircuit /> Explorez vos données</span><h2 id="questions-title">Une question suffit pour commencer.</h2><p>Sélectionnez un exemple pour alimenter la démonstration du chat. Aucun appel réel n’est envoyé.</p></div><div className="lp-question-grid" data-reveal>{questions.map((question, index) => <button type="button" key={question} onClick={() => selectQuestion(index)} className={active === index ? 'is-active' : ''}><span>{String(index + 1).padStart(2, '0')}</span><p>{question}</p><ArrowUpRight /></button>)}</div></div>
    </section>
  </>
}

function ProductTable() {
  return <div className="lp-chat-table"><div><b>Code</b><b>Produit</b><b>Statut</b></div><div><span>P001</span><span>Biscuit Démo</span><em>Actif</em></div><div><span>P002</span><span>Produit Pilote</span><em>Actif</em></div><div><span>P003</span><span>Prototype Test</span><em>Étude</em></div></div>
}

function AnalysisResult({ active }: { active: number }) {
  if (active === 4) return <div className="lp-chat-analysis"><div className="lp-chat-kpis"><span><small>Meilleur rendement</small><strong>EXP-25</strong></span><span><small>Temps inférieur</small><strong>EXP-25</strong></span><span><small>Coût maîtrisé</small><strong>EXP-25</strong></span></div><div className="lp-chat-source"><Database /> Sources : EXP-21 · EXP-25</div></div>
  if (active === 7) return <div className="lp-chat-analysis"><div className="lp-document-preview"><FileText /><span><strong>Résumé structuré</strong><small>Objectif · Méthode · Résultats · Points d’attention</small></span></div><div className="lp-chat-source"><Database /> Source : document de démonstration</div></div>
  return <div className="lp-chat-analysis"><div className="lp-chat-kpis"><span><small>Coût total</small><strong>128,40 DH</strong></span><span><small>Étape critique</small><strong>Cuisson</strong></span><span><small>Temps total</small><strong>2 h 45</strong></span></div><div className="lp-chat-mini-chart"><span><BarChart3 /> Coût par étape</span><div><i style={{ height: '38%' }} /><i style={{ height: '52%' }} /><i className="hot" style={{ height: '90%' }} /><i style={{ height: '48%' }} /><i style={{ height: '66%' }} /></div></div><div className="lp-chat-reco"><PackageSearch /><span><small>Recommandation simulée</small><strong>Vérifier le réglage thermique avant le prochain essai.</strong></span></div><div className="lp-chat-source"><Database /> Sources : Produit P001 · Processus · EXP-25</div></div>
}
