import { zodResolver } from '@hookform/resolvers/zod'
import { ImageIcon } from 'lucide-react'
import { useState, type ChangeEvent } from 'react'
import { useForm, type Resolver } from 'react-hook-form'
import { useApiMocks } from '../../../../api/apiMode'
import { productsApi } from '../../../../api/productsApi'
import { Button } from '../../../../components/ui/Button'
import { Input } from '../../../../components/ui/Input'
import { Select } from '../../../../components/ui/Select'
import { TextArea } from '../../../../components/ui/TextArea'
import { useProductCategories } from '../../hooks/useProductionReferences'
import { productSchema, type ProductFormValues } from '../../schemas/productSchema'
import { PRODUCTION_UNITS, type Product, type ProductFormInput } from '../../types/production.types'

const defaults: ProductFormValues = { name: '', internalReference: '', sapCode: '', category: '',
  description: '', imageUrl: null, themeColor: '#2563EB', targetSalePrice: 0,
  batchQuantity: 1000, productionUnit: 'kg', status: 'Draft' }
const colors = ['#2563EB', '#7C3AED', '#059669', '#D97706', '#DC2626', '#0891B2', '#DB2777']

function readFileAsDataUrl(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result))
    reader.onerror = () => reject(new Error("Impossible de lire l'image."))
    reader.readAsDataURL(file)
  })
}

export function ProductForm({ initialValues, onSubmit, loading }: {
  initialValues?: ProductFormInput | Product
  onSubmit: (value: ProductFormInput) => void
  loading?: boolean
}) {
  const categories = useProductCategories()
  const [uploadingImage, setUploadingImage] = useState(false)
  const [imageError, setImageError] = useState<string | null>(null)
  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema) as Resolver<ProductFormValues>,
    defaultValues: { ...defaults, ...initialValues, themeColor: initialValues?.themeColor ?? defaults.themeColor },
  })
  const imageUrl = watch('imageUrl')
  const themeColor = watch('themeColor')
  const categoryOptions = [{ label: categories.isLoading ? 'Chargement...' : 'Choisir une categorie', value: '' },
    ...(categories.data ?? []).map(item => ({ label: item.name, value: item.name }))]

  const uploadImage = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
      setImageError('Choisissez une image JPEG, PNG ou WebP de 5 Mo maximum.')
      return
    }
    setUploadingImage(true)
    setImageError(null)
    try {
      const url = useApiMocks ? await readFileAsDataUrl(file) : await productsApi.uploadImage(file)
      setValue('imageUrl', url, { shouldDirty: true, shouldValidate: true })
    } catch (error) {
      setImageError(error instanceof Error ? error.message : "Impossible d'envoyer l'image.")
    } finally {
      setUploadingImage(false)
    }
  }

  return <form onSubmit={handleSubmit(onSubmit)} className='space-y-6'>
    <section className='surface-card p-6'>
      <h2 className='font-bold text-slate-900'>Identification</h2>
      <p className='mt-1 mb-5 text-sm text-slate-500'>Informations de reference et categorie provenant de la base de donnees.</p>
      <div className='grid gap-4 md:grid-cols-2'>
        <Input label='Nom du produit' {...register('name')} error={errors.name?.message} />
        <Input label='Reference interne' {...register('internalReference')} error={errors.internalReference?.message} />
        <Input label='Code SAP' {...register('sapCode')} error={errors.sapCode?.message} />
        <Select label='Categorie' options={categoryOptions} disabled={categories.isLoading}
          {...register('category')} error={errors.category?.message ?? (categories.isError ? 'Categories indisponibles.' : undefined)} />
      </div>
      <div className='mt-4'><TextArea label='Description' rows={4} {...register('description')} error={errors.description?.message} /></div>
    </section>
    <section className='surface-card p-6'>
      <h2 className='font-bold text-slate-900'>Image et theme visuel</h2>
      <div className='mt-5 grid gap-5 md:grid-cols-[1fr_220px]'>
        <div className='space-y-4'>
          <Input label='URL image' type='url' placeholder='https://...' {...register('imageUrl')} error={errors.imageUrl?.message} />
          <Input label='Ou choisir une image' type='file' accept='image/jpeg,image/png,image/webp'
            onChange={uploadImage} disabled={uploadingImage}
            hint={uploadingImage ? 'Televersement en cours...' : 'JPEG, PNG ou WebP - 5 Mo maximum.'}
            error={imageError ?? undefined} />
          <div>
            <Input label='Couleur du theme' type='color' {...register('themeColor')} error={errors.themeColor?.message} />
            <div className='mt-2 flex flex-wrap gap-2'>{colors.map(color =>
              <button key={color} type='button' aria-label={`Choisir ${color}`} onClick={() => setValue('themeColor', color)}
                className='size-8 rounded-full border-2 border-white shadow ring-1 ring-slate-300'
                style={{ backgroundColor: color, outline: themeColor === color ? `3px solid ${color}` : undefined }} />)}
            </div>
          </div>
        </div>
        <div className='overflow-hidden rounded-2xl border-4 bg-slate-50' style={{ borderColor: themeColor }}>
          {imageUrl
            ? <img src={imageUrl} alt='Apercu du produit' className='h-44 w-full object-cover' />
            : <div className='grid h-44 place-items-center text-slate-400'><div className='text-center'><ImageIcon className='mx-auto size-8' /><span className='mt-2 block text-xs'>Aucune image</span></div></div>}
          <div className='p-3 text-sm font-semibold' style={{ color: themeColor }}>Apercu du theme</div>
        </div>
      </div>
    </section>
    <section className='surface-card p-6'>
      <h2 className='font-bold text-slate-900'>Parametres industriels</h2>
      <div className='mt-5 grid gap-4 md:grid-cols-3'>
        <Input label='Prix de vente cible (MAD)' type='number' step='0.01' {...register('targetSalePrice')} error={errors.targetSalePrice?.message} />
        <Input label='Quantite du lot' type='number' {...register('batchQuantity')} error={errors.batchQuantity?.message} />
        <Select label='Unite' options={PRODUCTION_UNITS.map(unit => ({ label: unit, value: unit }))}
          {...register('productionUnit')} error={errors.productionUnit?.message} />
      </div>
    </section>
    <div className='flex justify-end'><Button type='submit' loading={loading || uploadingImage}>Enregistrer le produit</Button></div>
  </form>
}
