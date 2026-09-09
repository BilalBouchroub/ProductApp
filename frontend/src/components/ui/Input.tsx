import { forwardRef, useId, type InputHTMLAttributes } from 'react'
import { cn } from '../../utils/cn'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  hint?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input({ id, label, error, hint, className, ...props }, ref) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const descriptionId = `${inputId}-description`
  return (
    <div className="w-full">
      {label && <label className="field-label" htmlFor={inputId}>{label}</label>}
      <input ref={ref} id={inputId} className={cn('field-control', error && 'border-red-400 focus:border-red-500 focus:ring-red-100', className)} aria-invalid={Boolean(error)} aria-describedby={error || hint ? descriptionId : undefined} {...props} />
      {(error || hint) && <p id={descriptionId} role={error ? 'alert' : undefined} className={cn('mt-1.5 text-xs', error ? 'text-red-600' : 'text-slate-500')}>{error ?? hint}</p>}
    </div>
  )
})
