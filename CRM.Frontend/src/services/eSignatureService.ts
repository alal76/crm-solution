import api from './apiClient';

export const getRequests = (params?: Record<string, any>) =>
  api.get('/esignature/requests', { params }).then((res) => res.data);

export const getRequest = (id: number) => api.get(`/esignature/requests/${id}`).then((res) => res.data);

export const createRequest = (payload: any) => api.post('/esignature/requests', payload).then((res) => res.data);

export const sendForSignature = (requestId: number) =>
  api.post(`/esignature/requests/${requestId}/send`).then((res) => res.data);

export const getSignatureStatus = (requestId: number) =>
  api.get(`/esignature/requests/${requestId}/status`).then((res) => res.data);

export const recordSignature = (requestId: number, signerId: string, signatureData: any) =>
  api.post(`/esignature/requests/${requestId}/signatures`, { signerId, signatureData }).then((res) => res.data);

export const downloadSignedDocument = (documentId: number) =>
  api.get(`/esignature/documents/${documentId}/download`, { responseType: 'blob' }).then((res) => res.data);

export const cancelRequest = (requestId: number, reason?: string) =>
  api.post(`/esignature/requests/${requestId}/cancel`, { reason }).then((res) => res.data);

export default {
  getRequests,
  getRequest,
  createRequest,
  sendForSignature,
  getSignatureStatus,
  recordSignature,
  downloadSignedDocument,
  cancelRequest,
};
