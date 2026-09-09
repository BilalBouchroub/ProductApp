import { describe, expect, it } from 'vitest'
import { productSchema } from './productSchema'

const validProduct = {
  name: 'Biscuit cacao',
  internalReference: 'BIS-001',
  sapCode: 'SAP-001',
  category: 'Biscuits',
  description: 'Biscuit industriel au cacao.',
  imageUrl: 'https://example.com/product.png',
  themeColor: '#2563EB',
  targetSalePrice: 12,
  batchQuantity: 1000,
  productionUnit: 'kg' as const,
  status: 'Draft' as const,
}

describe('productSchema', () => {
  it('accepte une image et une couleur hexadecimale', () => {
    expect(productSchema.safeParse(validProduct).success).toBe(true)
  })

  it('refuse une couleur non hexadecimale', () => {
    expect(productSchema.safeParse({ ...validProduct, themeColor: 'blue' }).success).toBe(false)
  })
})
