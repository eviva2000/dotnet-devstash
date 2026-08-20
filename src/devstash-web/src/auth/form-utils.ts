import type { FieldErrors } from './auth-types'

export function isValidEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)
}

export function firstError(errors: FieldErrors, field: string): string | undefined {
  return errors[field]?.[0]
}

export function focusFirstError(form: HTMLFormElement | null): void {
  requestAnimationFrame(() => {
    const invalidField = form?.querySelector<HTMLElement>('[aria-invalid="true"]')
    const summary = form?.querySelector<HTMLElement>('.error-summary')
    ;(invalidField ?? summary)?.focus()
  })
}
