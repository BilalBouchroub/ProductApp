import type { SapMaterialOption } from '../types/production.types'
export const mockSapMaterials: SapMaterialOption[] = [
  { sapCode: 'SAP-RM-FLR-001', designation: 'Farine de blé T55', unit: 'kg', unitCost: 4.2, availableStock: 4850, supplier: 'Minoteries du Maroc', batchNumber: 'FAR-260718', expirationDate: '2027-01-18' },
  { sapCode: 'SAP-RM-SUG-002', designation: 'Sucre semoule fin', unit: 'kg', unitCost: 6.1, availableStock: 920, supplier: 'Cosumar', batchNumber: 'SUC-260701', expirationDate: '2028-07-01' },
  { sapCode: 'SAP-RM-BUT-003', designation: 'Beurre pâtissier', unit: 'kg', unitCost: 48, availableStock: 110, supplier: 'Centrale Danone Pro', batchNumber: 'BEU-260725', expirationDate: '2026-09-15' },
  { sapCode: 'SAP-RM-COC-004', designation: 'Poudre de cacao 22/24', unit: 'kg', unitCost: 72, availableStock: 58, supplier: 'Cacao Industrie', batchNumber: 'CAC-260612', expirationDate: '2027-06-12' },
  { sapCode: 'SAP-RM-OAT-005', designation: 'Flocons d’avoine complets', unit: 'kg', unitCost: 13.5, availableStock: 420, supplier: 'Atlas Céréales', batchNumber: 'AVO-260710', expirationDate: '2027-02-10' },
  { sapCode: 'SAP-RM-DAT-006', designation: 'Pâte de dattes', unit: 'kg', unitCost: 28, availableStock: 75, supplier: 'Tafilalet Dattes', batchNumber: 'DAT-260720', expirationDate: '2027-01-20' },
  { sapCode: 'SAP-PK-FIL-011', designation: 'Film OPP imprimé 250 mm', unit: 'm', unitCost: 0.42, availableStock: 2300, supplier: 'Pack Maroc', batchNumber: 'OPP-260601', expirationDate: null },
  { sapCode: 'SAP-RM-VAN-012', designation: 'Arôme naturel vanille', unit: 'L', unitCost: 155, availableStock: 12, supplier: 'Aromaflor', batchNumber: 'VAN-260515', expirationDate: '2026-11-15' },
]
