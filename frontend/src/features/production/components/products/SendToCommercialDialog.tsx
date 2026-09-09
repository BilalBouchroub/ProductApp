import { Button } from '../../../../components/ui/Button'

export function SendToCommercialDialog(props: { open:boolean; name:string; loading?:boolean; onClose:()=>void; onConfirm:()=>void }) {
  if (!props.open) return null
  return <section role='dialog' aria-label='Envoyer le produit' className='fixed right-4 bottom-4 left-4 z-40 max-w-lg rounded-2xl border-2 border-blue-400 bg-blue-50 p-5 shadow-lg sm:left-auto sm:w-full'>
    <h2 className='font-semibold text-slate-950'>Envoyer vers l&apos;&eacute;tude commerciale</h2>
    <p className='mt-1 text-sm text-slate-600'>La compl&eacute;tude de {props.name} sera v&eacute;rifi&eacute;e avant sa transmission.</p>
    <div className='mt-4 flex gap-3'>
      <Button variant='secondary' onClick={props.onClose}>Annuler</Button>
      <Button loading={props.loading} onClick={props.onConfirm}>V&eacute;rifier et envoyer</Button>
    </div>
  </section>
}
