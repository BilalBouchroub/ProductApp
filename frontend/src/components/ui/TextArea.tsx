import { forwardRef, useId, type TextareaHTMLAttributes } from 'react'
import { cn } from '../../utils/cn'

interface TextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> { label?: string; error?: string; hint?: string }

export const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(function TextArea({ id, label, error, hint, className, ...props }, ref) {
  const generatedId = useId()
  const textAreaId = id ?? generatedId
  return <div className="w-full">
    {label && <label className="field-label" htmlFor={textAreaId}>{label}</label>}
    <textarea ref={ref} id={textAreaId} className={cn('field-control min-h-28 resize-y', error && 'border-red-400', className)} aria-invalid={Boolean(error)} aria-describedby={error || hint ? `${textAreaId}-description` : undefined} {...props} />
    {(error || hint) && <p id={`${textAreaId}-description`} role={error ? 'alert' : undefined} className={cn('mt-1.5 text-xs', error ? 'text-red-600' : 'text-slate-500')}>{error ?? hint}</p>}
  </div>
})
