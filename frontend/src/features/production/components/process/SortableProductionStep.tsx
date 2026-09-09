import { CSS } from '@dnd-kit/utilities'
import { useSortable } from '@dnd-kit/sortable'
import type { ComponentProps } from 'react'
import { ProductionStepCard } from './ProductionStepCard'

export function SortableProductionStep(props: ComponentProps<typeof ProductionStepCard> & { id: string }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: props.id })
  return <div ref={setNodeRef} className={`w-full sm:w-auto ${isDragging ? 'opacity-70' : ''}`}
    style={{ transform: CSS.Transform.toString(transform), transition, zIndex: isDragging ? 20 : undefined }}>
    <ProductionStepCard {...props} dragHandleProps={{ ...attributes, ...listeners }} />
  </div>
}
