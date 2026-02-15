/**
 * Address Management Types and Interfaces
 */

export type AddressType = 'Billing' | 'Shipping' | 'Primary' | 'Other';

export interface Address {
  id: number;
  line1: string;
  line2?: string;
  city: string;
  state?: string;
  zipCode?: string;
  country: string;
  label?: string;
  addressType: AddressType;
  isPrimary: boolean;
  isPrimaryBilling?: boolean;
  isPrimaryShipping?: boolean;
  createdAt?: string;
  updatedAt?: string;
  isDeleted?: boolean;
}

export interface CreateAddressDto {
  line1: string;
  line2?: string;
  city: string;
  state?: string;
  zipCode?: string;
  country: string;
  label?: string;
  addressType: AddressType;
  isPrimary?: boolean;
}

export interface UpdateAddressDto {
  line1?: string;
  line2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  label?: string;
  addressType?: AddressType;
  isPrimary?: boolean;
}

export const ADDRESS_TYPES: AddressType[] = ['Billing', 'Shipping', 'Primary', 'Other'];
