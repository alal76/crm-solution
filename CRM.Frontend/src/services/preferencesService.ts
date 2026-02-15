import apiClient from './apiClient';

export interface PreferencesDto {
  id?: number;
  optInEmail: boolean;
  optInSms: boolean;
  optInPhone: boolean;
  optInPostal: boolean;
  preferredContactMethod?: string;
  preferredLanguage?: string;
  timezone?: string;
  doNotCallDate?: string | null;
  doNotEmailDate?: string | null;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface ContactPreferencesDto {
  useCustomPreferences: boolean;
  preferences: PreferencesDto;
}

const toDateOnly = (value?: string | null) => {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  return date.toISOString().slice(0, 10);
};

export const preferencesService = {
  getAccountPreferences: async (accountId: number) => {
    const response = await apiClient.get<PreferencesDto>(`/accounts/${accountId}/preferences`);
    const data = response.data;
    return {
      ...data,
      doNotCallDate: data.doNotCallDate ? toDateOnly(data.doNotCallDate) : null,
      doNotEmailDate: data.doNotEmailDate ? toDateOnly(data.doNotEmailDate) : null,
    } as PreferencesDto;
  },

  updateAccountPreferences: (accountId: number, dto: PreferencesDto) =>
    apiClient.put<PreferencesDto>(`/accounts/${accountId}/preferences`, dto),

  getContactPreferences: async (contactId: number) => {
    const response = await apiClient.get<ContactPreferencesDto>(`/contacts/${contactId}/preferences`);
    const preferences = response.data.preferences;
    return {
      ...response.data,
      preferences: {
        ...preferences,
        doNotCallDate: preferences.doNotCallDate ? toDateOnly(preferences.doNotCallDate) : null,
        doNotEmailDate: preferences.doNotEmailDate ? toDateOnly(preferences.doNotEmailDate) : null,
      },
    } as ContactPreferencesDto;
  },
};
