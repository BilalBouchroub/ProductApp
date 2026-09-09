import { Button } from '../../../../components/ui/Button'

export function DuplicateProductDialog(props: { open:boolean; name:string; loading?:boolean; onClose:()=>void; onConfirm:()=>void }) {
  if (!props.open) return null
  return <section role='dialog' aria-label='Dupliquer le produit' className='fixed right-4 bottom-4 left-4 z-40 max-w-lg rounded-2xl border-2 border-blue-400 bg-blue-50 p-5 shadow-lg sm:left-auto sm:w-full'>
    <h2 className='font-semibold text-slate-950'>Dupliquer le produit</h2>
    <p className='mt-1 text-sm text-slate-600'>Une copie en brouillon de {props.name} sera cr&eacute;&eacute;e avec sa cha&icirc;ne de production.</p>
    <div className='mt-4 flex gap-3'>
      <Button variant='secondary' onClick={props.onClose}>Annuler</Button>
      <Button loading={props.loading} onClick={props.onConfirm}>Dupliquer</Button>
    </div>
  </section>
}
