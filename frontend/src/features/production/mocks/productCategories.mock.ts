import type { ProductCategoryOption } from '../types/production.types'

export const mockProductCategories: ProductCategoryOption[] = [
  { id: 'cat-biscuits', code: 'BISCUITS', name: 'Biscuits', description: 'Produits biscuitiers' },
  { id: 'cat-electronics', code: 'ELECTRONICS', name: 'Electronique', description: 'Produits electroniques' },
  { id: 'cat-food', code: 'FOOD', name: 'Alimentaire', description: 'Produits alimentaires' },
  { id: 'cat-beverages', code: 'BEVERAGES', name: 'Boissons', description: 'Boissons et liquides' },
  { id: 'cat-cosmetics', code: 'COSMETICS', name: 'Cosmetiques', description: 'Produits cosmetiques' },
  { id: 'cat-textiles', code: 'TEXTILES', name: 'Textile', description: 'Produits textiles' },
  { id: 'cat-other', code: 'OTHER', name: 'Autres', description: 'Autres produits industriels' },
]
