import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { mockProducts } from '../../mocks/products.mock'
import { ProductTable } from './ProductTable'

const product = mockProducts[0]!
const draft = { ...product, status: 'Draft' as const }
const visualProduct = { ...product, imageUrl: 'https://example.com/biscuit.png', themeColor: '#DB2777' }

describe('ProductTable', () => {
  it('affiche la photo et le theme choisis pour differencier le produit', () => {
    const { container } = render(<ProductTable products={[visualProduct]} onDuplicate={vi.fn()} onSend={vi.fn()} onDelete={vi.fn()} />)

    expect(screen.getByRole('img', { name: `Image de ${visualProduct.name}` })).toHaveAttribute('src', visualProduct.imageUrl)
    expect(container.querySelector('article')).toHaveAttribute('data-theme-color', visualProduct.themeColor)
    expect(screen.getByText(visualProduct.category)).toHaveStyle({ color: visualProduct.themeColor })
  })

  it('utilise des liens directs pour voir et modifier', () => {
    render(<ProductTable products={[product]} onDuplicate={vi.fn()} onSend={vi.fn()} onDelete={vi.fn()} />)
    expect(screen.getByRole('link', { name: `Consulter ${product.name}` })).toHaveAttribute('href', `/production/products/${product.id}`)
    expect(screen.getByRole('link', { name: `Modifier ${product.name}` })).toHaveAttribute('href', `/production/products/${product.id}/edit`)
    expect(screen.queryByRole('button', { name: `Supprimer ${product.name}` })).not.toBeInTheDocument()
  })

  it('garde les actions independantes', async () => {
    const user = userEvent.setup()
    const duplicate = vi.fn()
    const send = vi.fn()
    render(<ProductTable products={[product]} onDuplicate={duplicate} onSend={send} onDelete={vi.fn()} />)
    await user.click(screen.getByRole('button', { name: `Dupliquer ${product.name}` }))
    await user.click(screen.getByRole('button', { name: `Envoyer ${product.name} au commercial` }))
    expect(duplicate).toHaveBeenCalledWith(product)
    expect(send).toHaveBeenCalledWith(product)
  })

  it('permet de supprimer uniquement un brouillon', async () => {
    const user = userEvent.setup()
    const remove = vi.fn()
    render(<ProductTable products={[draft]} onDuplicate={vi.fn()} onSend={vi.fn()} onDelete={remove} />)
    await user.click(screen.getByRole('button', { name: `Supprimer ${draft.name}` }))
    expect(remove).toHaveBeenCalledWith(draft)
  })
})
