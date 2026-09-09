import type { EquipmentOption } from '../types/production.types'
export const mockEquipment: EquipmentOption[] = [
  { id: 'eq-001', sapCode: 'SAP-EQ-MIX-01', name: 'Mélangeur horizontal 500 L', category: 'Machine', status: 'Available', hourlyCost: 185 },
  { id: 'eq-002', sapCode: 'SAP-EQ-KND-02', name: 'Pétrin industriel spirale', category: 'Machine', status: 'Available', hourlyCost: 210 },
  { id: 'eq-003', sapCode: 'SAP-EQ-MOL-01', name: 'Façonneuse rotative', category: 'Machine', status: 'Available', hourlyCost: 240 },
  { id: 'eq-004', sapCode: 'SAP-EQ-OVN-02', name: 'Four tunnel ligne 02', category: 'Machine', status: 'Maintenance', hourlyCost: 520 },
  { id: 'eq-005', sapCode: 'SAP-EQ-COO-01', name: 'Convoyeur de refroidissement', category: 'Equipment', status: 'Available', hourlyCost: 130 },
  { id: 'eq-006', sapCode: 'SAP-EQ-PAC-03', name: 'Ensacheuse horizontale', category: 'Machine', status: 'Available', hourlyCost: 275 },
  { id: 'eq-007', sapCode: 'SAP-EQ-SCL-04', name: 'Balance industrielle connectée', category: 'Equipment', status: 'Available', hourlyCost: 45 },
]
