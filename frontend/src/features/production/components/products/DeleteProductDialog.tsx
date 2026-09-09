import { Button } from '../../../../components/ui/Button'

export function DeleteProductDialog(props: { open:boolean; name:string; loading?:boolean; onClose:()=>void; onConfirm:()=>void }) {
  if (!props.open) return null
  return <section role='dialog' aria-label='Supprimer le produit' className='fixed right-4 bottom-4 left-4 z-40 max-w-lg rounded-2xl border-2 border-red-400 bg-red-50 p-5 shadow-lg sm:left-auto sm:w-full'>
    <h2 className='font-semibold text-slate-950'>Supprimer le produit</h2>
    <p className='mt-1 text-sm text-slate-600'>Supprimer d&eacute;finitivement le brouillon {props.name} et ses donn&eacute;es associ&eacute;es&nbsp;?</p>
    <div className='mt-4 flex gap-3'>
      <Button variant='secondary' onClick={props.onClose}>Annuler</Button>
      <Button variant='danger' loading={props.loading} onClick={props.onConfirm}>Supprimer</Button>
    </div>
  </section>
}
