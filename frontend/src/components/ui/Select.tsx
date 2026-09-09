import { forwardRef, useId, type SelectHTMLAttributes } from 'react'
import { cn } from '../../utils/cn'

export interface SelectOption { label: string; value: string }
interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> { label?: string; error?: string; options: readonly SelectOption[] }

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select({ id, label, error, options, className, ...props }, ref) {
  const generatedId = useId()
  const selectId = id ?? generatedId
  return <div className="w-full">
    {label && <label className="field-label" htmlFor={selectId}>{label}</label>}
    <select ref={ref} id={selectId} className={cn('field-control', error && 'border-red-400', className)} aria-invalid={Boolean(error)} aria-describedby={error ? `${selectId}-error` : undefined} {...props}>
      {options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
    </select>
    {error && <p id={`${selectId}-error`} role="alert" className="mt-1.5 text-xs text-red-600">{error}</p>}
  </div>
})
