/**
 * Contact Info Components
 * 
 * Reusable components for managing addresses, phone numbers, emails, 
 * and social media accounts across Customer, Contact, Lead, and Account entities.
 */

export { default as AddressManager } from './AddressManager';
export { default as PhoneManager } from './PhoneManager';
export { default as EmailManager } from './EmailManager';
export { default as SocialMediaManager } from './SocialMediaManager';
export { default as ContactInfoPanel } from './ContactInfoPanel';
export { default as ShareContactInfoModal } from './ShareContactInfoModal';

// Re-export types for convenience
export type {
  EntityType,
  AddressType,
  PhoneType,
  EmailType,
  SocialMediaPlatform,
  SocialMediaAccountType,
  AddressDto,
  LinkedAddressDto,
  CreateAddressDto,
  LinkAddressDto,
  PhoneNumberDto,
  LinkedPhoneDto,
  CreatePhoneNumberDto,
  LinkPhoneDto,
  EmailAddressDto,
  LinkedEmailDto,
  CreateEmailAddressDto,
  LinkEmailDto,
  SocialMediaAccountDto,
  LinkedSocialMediaDto,
  CreateSocialMediaAccountDto,
  LinkSocialMediaDto,
  EntityContactInfoDto,
  ShareContactInfoDto,
} from '../../services/contactInfoService';
