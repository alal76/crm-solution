import apiClient from './apiClient';

const accountService = {
  uploadContract: (accountId: number, file: File) => {
    const form = new FormData();
    form.append('file', file, file.name);
    return apiClient.post(`/accounts/${accountId}/upload-contract`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  downloadContractUrl: (accountId: number) => {
    return `${(apiClient.defaults.baseURL || '').replace(/\/api$/, '')}/api/accounts/${accountId}/contract`;
  },

  deleteContract: (accountId: number) => apiClient.delete(`/accounts/${accountId}/contract`),
};

export default accountService;
