import apiClient from './apiClient';

// ============================================================
// ENUMS
// ============================================================

export type EntityType = 'Customer' | 'Contact' | 'Lead' | 'Account';
export type AddressType = 'Primary' | 'Billing' | 'Shipping' | 'Mailing' | 'Headquarters' | 'Branch' | 'Warehouse' | 'Other';
export type PhoneType = 'Mobile' | 'Home' | 'Office' | 'Direct' | 'Fax' | 'Toll-Free' | 'Pager' | 'Other';
export type EmailType = 'Personal' | 'Work' | 'Invoicing' | 'Support' | 'Marketing' | 'General' | 'Other';
export type SocialMediaPlatform = 'LinkedIn' | 'Twitter' | 'Facebook' | 'Instagram' | 'YouTube' | 'TikTok' | 'Pinterest' | 'WhatsApp' | 'Telegram' | 'WeChat' | 'Slack' | 'Discord' | 'GitHub' | 'Other';
export type SocialMediaAccountType = 'Personal' | 'Business' | 'Official' | 'Support';
export type EngagementLevel = 'VeryLow' | 'Low' | 'Medium' | 'High' | 'VeryHigh';

// ============================================================
// ADDRESS TYPES
// ============================================================

export interface AddressDto {
  id: number;
  label?: string;
  line1: string;
  line2?: string;
  line3?: string;
  city: string;
  state: string;
  postalCode: string;
  county?: string;
  countryCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  geocodeAccuracy?: string;
  isVerified?: boolean;
  verifiedDate?: string;
  verificationSource?: string;
  isResidential?: boolean;
  deliveryInstructions?: string;
  accessHours?: string;
  siteContactName?: string;
  siteContactPhone?: string;
  notes?: string;
  formattedAddress?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface LinkedAddressDto extends AddressDto {
  linkId: number;
  addressType: AddressType;
  isPrimary: boolean;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  linkNotes?: string;
}

export interface CreateAddressDto {
  label?: string;
  line1: string;
  line2?: string;
  line3?: string;
  city: string;
  state: string;
  postalCode: string;
  county?: string;
  countryCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  isResidential?: boolean;
  deliveryInstructions?: string;
  accessHours?: string;
  siteContactName?: string;
  siteContactPhone?: string;
  notes?: string;
}

export interface LinkAddressDto {
  addressId?: number;
  newAddress?: CreateAddressDto;
  entityType: EntityType;
  entityId: number;
  addressType: AddressType;
  isPrimary?: boolean;
  validFrom?: string;
  validTo?: string;
  notes?: string;
}

// ============================================================
// PHONE NUMBER TYPES
// ============================================================

export interface PhoneNumberDto {
  id: number;
  label?: string;
  countryCode: string;
  areaCode?: string;
  number: string;
  extension?: string;
  formattedNumber?: string;
  canSMS?: boolean;
  canWhatsApp?: boolean;
  canFax?: boolean;
  isVerified?: boolean;
  verifiedDate?: string;
  bestTimeToCall?: string;
  notes?: string;
  fullNumber?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface LinkedPhoneDto extends PhoneNumberDto {
  linkId: number;
  phoneType: PhoneType;
  isPrimary: boolean;
  doNotCall: boolean;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  linkNotes?: string;
}

export interface CreatePhoneNumberDto {
  label?: string;
  countryCode: string;
  areaCode?: string;
  number: string;
  extension?: string;
  canSMS?: boolean;
  canWhatsApp?: boolean;
  canFax?: boolean;
  bestTimeToCall?: string;
  notes?: string;
}

export interface LinkPhoneDto {
  phoneId?: number;
  newPhone?: CreatePhoneNumberDto;
  entityType: EntityType;
  entityId: number;
  phoneType: PhoneType;
  isPrimary?: boolean;
  doNotCall?: boolean;
  validFrom?: string;
  validTo?: string;
  notes?: string;
}

// ============================================================
// EMAIL ADDRESS TYPES
// ============================================================

export interface EmailAddressDto {
  id: number;
  label?: string;
  email: string;
  displayName?: string;
  isVerified?: boolean;
  verifiedDate?: string;
  bounceCount?: number;
  lastBounceDate?: string;
  hardBounce?: boolean;
  lastEmailSent?: string;
  lastEmailOpened?: string;
  emailEngagementScore?: number;
  isDeliverable: boolean;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface LinkedEmailDto extends EmailAddressDto {
  linkId: number;
  emailType: EmailType;
  isPrimary: boolean;
  doNotEmail: boolean;
  unsubscribedDate?: string;
  marketingOptIn: boolean;
  transactionalOnly: boolean;
  canSendMarketing: boolean;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  linkNotes?: string;
}

export interface CreateEmailAddressDto {
  label?: string;
  email: string;
  displayName?: string;
  notes?: string;
}

export interface LinkEmailDto {
  emailId?: number;
  newEmail?: CreateEmailAddressDto;
  entityType: EntityType;
  entityId: number;
  emailType: EmailType;
  isPrimary?: boolean;
  doNotEmail?: boolean;
  marketingOptIn?: boolean;
  transactionalOnly?: boolean;
  validFrom?: string;
  validTo?: string;
  notes?: string;
}

export interface EmailPreferencesDto {
  doNotEmail: boolean;
  marketingOptIn: boolean;
  transactionalOnly: boolean;
}

// ============================================================
// SOCIAL MEDIA TYPES
// ============================================================

export interface SocialMediaAccountDto {
  id: number;
  platform: SocialMediaPlatform;
  platformOther?: string;
  accountType: SocialMediaAccountType;
  handleOrUsername: string;
  profileUrl?: string;
  displayName?: string;
  followerCount?: number;
  followingCount?: number;
  isVerifiedAccount?: boolean;
  isActive?: boolean;
  lastActivityDate?: string;
  engagementLevel?: EngagementLevel;
  platformName?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface LinkedSocialMediaDto extends SocialMediaAccountDto {
  linkId: number;
  isPrimary: boolean;
  preferredForContact: boolean;
  linkNotes?: string;
}

export interface CreateSocialMediaAccountDto {
  platform: SocialMediaPlatform;
  platformOther?: string;
  accountType: SocialMediaAccountType;
  handleOrUsername: string;
  profileUrl?: string;
  displayName?: string;
  followerCount?: number;
  followingCount?: number;
  isVerifiedAccount?: boolean;
  notes?: string;
}

export interface LinkSocialMediaDto {
  socialMediaAccountId?: number;
  newSocialMedia?: CreateSocialMediaAccountDto;
  entityType: EntityType;
  entityId: number;
  isPrimary?: boolean;
  preferredForContact?: boolean;
  notes?: string;
}

// ============================================================
// AGGREGATE TYPES
// ============================================================

export interface EntityContactInfoDto {
  entityType: EntityType;
  entityId: number;
  entityName?: string;
  addresses: LinkedAddressDto[];
  phoneNumbers: LinkedPhoneDto[];
  emailAddresses: LinkedEmailDto[];
  socialMediaAccounts: LinkedSocialMediaDto[];
}

export interface ShareContactInfoDto {
  targetEntityType: EntityType;
  targetEntityId: number;
  addressIds?: number[];
  phoneIds?: number[];
  emailIds?: number[];
  socialMediaIds?: number[];
  setAsPrimary?: boolean;
  defaultAddressType?: AddressType;
  defaultPhoneType?: PhoneType;
  defaultEmailType?: EmailType;
}

export interface EntityReference {
  entityType: EntityType;
  entityId: number;
  entityName: string;
}

// ============================================================
// CONTACT INFO SERVICE
// ============================================================

const BASE_URL = '/contact-info';

export const contactInfoService = {
  // ============================================================
  // AGGREGATE OPERATIONS
  // ============================================================
  
  /**
   * Get all contact information for an entity
   */
  getEntityContactInfo: (entityType: EntityType, entityId: number) =>
    apiClient.get<EntityContactInfoDto>(`${BASE_URL}/${entityType}/${entityId}`),

  /**
   * Share contact information with another entity
   */
  shareContactInfo: (dto: ShareContactInfoDto) =>
    apiClient.post(`${BASE_URL}/share`, dto),

  // ============================================================
  // ADDRESS OPERATIONS
  // ============================================================

  /**
   * Get all addresses for an entity
   */
  getAddresses: (entityType: EntityType, entityId: number) =>
    apiClient.get<LinkedAddressDto[]>(`${BASE_URL}/${entityType}/${entityId}/addresses`),

  /**
   * Get an address by ID
   */
  getAddressById: (addressId: number) =>
    apiClient.get<AddressDto>(`${BASE_URL}/addresses/${addressId}`),

  /**
   * Get entities sharing an address
   */
  getEntitiesSharingAddress: (addressId: number) =>
    apiClient.get<EntityReference[]>(`${BASE_URL}/addresses/${addressId}/shared-by`),

  /**
   * Create a new standalone address
   */
  createAddress: (dto: CreateAddressDto) =>
    apiClient.post<AddressDto>(`${BASE_URL}/addresses`, dto),

  /**
   * Link an address to an entity (creates new address if newAddress provided)
   */
  linkAddress: (dto: LinkAddressDto) =>
    apiClient.post<LinkedAddressDto>(`${BASE_URL}/addresses/link`, dto),

  /**
   * Update an address
   */
  updateAddress: (addressId: number, dto: CreateAddressDto) =>
    apiClient.put<AddressDto>(`${BASE_URL}/addresses/${addressId}`, dto),

  /**
   * Unlink an address from an entity
   */
  unlinkAddress: (linkId: number) =>
    apiClient.delete(`${BASE_URL}/addresses/link/${linkId}`),

  /**
   * Delete an address permanently
   */
  deleteAddress: (addressId: number) =>
    apiClient.delete(`${BASE_URL}/addresses/${addressId}`),

  /**
   * Set an address as primary
   */
  setPrimaryAddress: (entityType: EntityType, entityId: number, addressId: number, addressType?: AddressType) =>
    apiClient.post(`${BASE_URL}/${entityType}/${entityId}/addresses/${addressId}/set-primary?addressTypeStr=${addressType || 'Primary'}`),

  // ============================================================
  // PHONE OPERATIONS
  // ============================================================

  /**
   * Get all phone numbers for an entity
   */
  getPhoneNumbers: (entityType: EntityType, entityId: number) =>
    apiClient.get<LinkedPhoneDto[]>(`${BASE_URL}/${entityType}/${entityId}/phones`),

  /**
   * Get a phone number by ID
   */
  getPhoneNumberById: (phoneId: number) =>
    apiClient.get<PhoneNumberDto>(`${BASE_URL}/phones/${phoneId}`),

  /**
   * Get entities sharing a phone number
   */
  getEntitiesSharingPhone: (phoneId: number) =>
    apiClient.get<EntityReference[]>(`${BASE_URL}/phones/${phoneId}/shared-by`),

  /**
   * Create a new standalone phone number
   */
  createPhoneNumber: (dto: CreatePhoneNumberDto) =>
    apiClient.post<PhoneNumberDto>(`${BASE_URL}/phones`, dto),

  /**
   * Link a phone number to an entity
   */
  linkPhoneNumber: (dto: LinkPhoneDto) =>
    apiClient.post<LinkedPhoneDto>(`${BASE_URL}/phones/link`, dto),

  /**
   * Update a phone number
   */
  updatePhoneNumber: (phoneId: number, dto: CreatePhoneNumberDto) =>
    apiClient.put<PhoneNumberDto>(`${BASE_URL}/phones/${phoneId}`, dto),

  /**
   * Unlink a phone number from an entity
   */
  unlinkPhoneNumber: (linkId: number) =>
    apiClient.delete(`${BASE_URL}/phones/link/${linkId}`),

  /**
   * Delete a phone number permanently
   */
  deletePhoneNumber: (phoneId: number) =>
    apiClient.delete(`${BASE_URL}/phones/${phoneId}`),

  /**
   * Set a phone number as primary
   */
  setPrimaryPhone: (entityType: EntityType, entityId: number, phoneId: number, phoneType?: PhoneType) =>
    apiClient.post(`${BASE_URL}/${entityType}/${entityId}/phones/${phoneId}/set-primary?phoneTypeStr=${phoneType || 'Office'}`),

  // ============================================================
  // EMAIL OPERATIONS
  // ============================================================

  /**
   * Get all email addresses for an entity
   */
  getEmailAddresses: (entityType: EntityType, entityId: number) =>
    apiClient.get<LinkedEmailDto[]>(`${BASE_URL}/${entityType}/${entityId}/emails`),

  /**
   * Get an email address by ID
   */
  getEmailAddressById: (emailId: number) =>
    apiClient.get<EmailAddressDto>(`${BASE_URL}/emails/${emailId}`),

  /**
   * Find an email by address string
   */
  findEmailByAddress: (email: string) =>
    apiClient.get<EmailAddressDto>(`${BASE_URL}/emails/find?email=${encodeURIComponent(email)}`),

  /**
   * Get entities sharing an email
   */
  getEntitiesSharingEmail: (emailId: number) =>
    apiClient.get<EntityReference[]>(`${BASE_URL}/emails/${emailId}/shared-by`),

  /**
   * Create a new standalone email address
   */
  createEmailAddress: (dto: CreateEmailAddressDto) =>
    apiClient.post<EmailAddressDto>(`${BASE_URL}/emails`, dto),

  /**
   * Link an email address to an entity
   */
  linkEmailAddress: (dto: LinkEmailDto) =>
    apiClient.post<LinkedEmailDto>(`${BASE_URL}/emails/link`, dto),

  /**
   * Update an email address
   */
  updateEmailAddress: (emailId: number, dto: CreateEmailAddressDto) =>
    apiClient.put<EmailAddressDto>(`${BASE_URL}/emails/${emailId}`, dto),

  /**
   * Update email preferences for a link
   */
  updateEmailPreferences: (linkId: number, preferences: EmailPreferencesDto) =>
    apiClient.put(`${BASE_URL}/emails/link/${linkId}/preferences`, preferences),

  /**
   * Unlink an email address from an entity
   */
  unlinkEmailAddress: (linkId: number) =>
    apiClient.delete(`${BASE_URL}/emails/link/${linkId}`),

  /**
   * Delete an email address permanently
   */
  deleteEmailAddress: (emailId: number) =>
    apiClient.delete(`${BASE_URL}/emails/${emailId}`),

  /**
   * Set an email as primary
   */
  setPrimaryEmail: (entityType: EntityType, entityId: number, emailId: number, emailType?: EmailType) =>
    apiClient.post(`${BASE_URL}/${entityType}/${entityId}/emails/${emailId}/set-primary?emailTypeStr=${emailType || 'General'}`),

  // ============================================================
  // SOCIAL MEDIA OPERATIONS
  // ============================================================

  /**
   * Get all social media accounts for an entity
   */
  getSocialMediaAccounts: (entityType: EntityType, entityId: number) =>
    apiClient.get<LinkedSocialMediaDto[]>(`${BASE_URL}/${entityType}/${entityId}/social-media`),

  /**
   * Get a social media account by ID
   */
  getSocialMediaAccountById: (socialMediaId: number) =>
    apiClient.get<SocialMediaAccountDto>(`${BASE_URL}/social-media/${socialMediaId}`),

  /**
   * Create a new standalone social media account
   */
  createSocialMediaAccount: (dto: CreateSocialMediaAccountDto) =>
    apiClient.post<SocialMediaAccountDto>(`${BASE_URL}/social-media`, dto),

  /**
   * Link a social media account to an entity
   */
  linkSocialMediaAccount: (dto: LinkSocialMediaDto) =>
    apiClient.post<LinkedSocialMediaDto>(`${BASE_URL}/social-media/link`, dto),

  /**
   * Update a social media account
   */
  updateSocialMediaAccount: (socialMediaId: number, dto: CreateSocialMediaAccountDto) =>
    apiClient.put<SocialMediaAccountDto>(`${BASE_URL}/social-media/${socialMediaId}`, dto),

  /**
   * Unlink a social media account from an entity
   */
  unlinkSocialMediaAccount: (linkId: number) =>
    apiClient.delete(`${BASE_URL}/social-media/link/${linkId}`),

  /**
   * Delete a social media account permanently
   */
  deleteSocialMediaAccount: (socialMediaId: number) =>
    apiClient.delete(`${BASE_URL}/social-media/${socialMediaId}`),

  /**
   * Set a social media account as primary
   */
  setPrimarySocialMedia: (entityType: EntityType, entityId: number, socialMediaId: number) =>
    apiClient.post(`${BASE_URL}/${entityType}/${entityId}/social-media/${socialMediaId}/set-primary`),
};

export default contactInfoService;
