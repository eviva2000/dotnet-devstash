import type { ReactNode } from 'react'

export function AuthLayout({
  eyebrow,
  title,
  intro,
  children,
  footer,
}: {
  eyebrow: string
  title: string
  intro: string
  children: ReactNode
  footer: ReactNode
}) {
  return (
    <main className="auth-page">
      <section className="auth-card" aria-labelledby="auth-title">
        <div className="brand-mark" aria-hidden="true">
          <span>&lt;/&gt;</span>
        </div>
        <p className="eyebrow">{eyebrow}</p>
        <h1 id="auth-title">{title}</h1>
        <p className="auth-intro">{intro}</p>
        {children}
        <p className="auth-footer">{footer}</p>
      </section>
    </main>
  )
}

export function ErrorSummary({ message }: { message: string }) {
  return (
    <div className="error-summary" role="alert" tabIndex={-1}>
      <span aria-hidden="true">!</span>
      <p>{message}</p>
    </div>
  )
}
