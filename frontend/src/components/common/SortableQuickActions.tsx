import { DndContext, KeyboardSensor, PointerSensor, closestCenter, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core'
import { SortableContext, arrayMove, rectSortingStrategy, sortableKeyboardCoordinates, useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { GripVertical, type LucideIcon } from 'lucide-react'
import { useState } from 'react'

export interface QuickAction { id: string; label: string; description: string; icon: LucideIcon }
function SortableAction({ action }: { action: QuickAction }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: action.id })
  const Icon = action.icon
  return <div ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition }} className={`rounded-xl border border-slate-200 bg-white p-4 ${isDragging ? 'z-10 shadow-xl' : 'shadow-sm'}`}><div className="flex items-start justify-between"><span className="rounded-lg bg-blue-50 p-2 text-blue-600"><Icon className="size-4" /></span><button type="button" {...attributes} {...listeners} className="cursor-grab rounded-md p-1 text-slate-400 hover:bg-slate-100 active:cursor-grabbing" aria-label={`Réorganiser ${action.label}`}><GripVertical className="size-4" /></button></div><p className="mt-3 text-sm font-semibold text-slate-800">{action.label}</p><p className="mt-1 text-xs leading-5 text-slate-500">{action.description}</p></div>
}
export function SortableQuickActions({ initialActions }: { initialActions: readonly QuickAction[] }) {
  const [actions, setActions] = useState([...initialActions])
  const sensors = useSensors(useSensor(PointerSensor), useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }))
  const handleDragEnd = ({ active, over }: DragEndEvent) => { if (!over || active.id === over.id) return; setActions((items) => { const oldIndex = items.findIndex((item) => item.id === active.id); const newIndex = items.findIndex((item) => item.id === over.id); return arrayMove(items, oldIndex, newIndex) }) }
  return <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}><SortableContext items={actions} strategy={rectSortingStrategy}><div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{actions.map((action) => <SortableAction key={action.id} action={action} />)}</div></SortableContext></DndContext>
}
