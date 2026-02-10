import api from 'src/services/api';

export const getRequests = (params?: Record<string, any>) =>
  api.get('/api/esignature/requests', { params }).then((res) => res.data);

export const getRequest = (id: number) => api.get(`/api/esignature/requests/${id}`).then((res) => res.data);

export const createRequest = (payload: any) => api.post('/api/esignature/requests', payload).then((res) => res.data);

export const sendForSignature = (requestId: number) =>
  api.post(`/api/esignature/requests/${requestId}/send`).then((res) => res.data);

export const getSignatureStatus = (requestId: number) =>
  api.get(`/api/esignature/requests/${requestId}/status`).then((res) => res.data);

export const recordSignature = (requestId: number, signerId: string, signatureData: any) =>
  api.post(`/api/esignature/requests/${requestId}/signatures`, { signerId, signatureData }).then((res) => res.data);

export const downloadSignedDocument = (documentId: number) =>
  api.get(`/api/esignature/documents/${documentId}/download`, { responseType: 'blob' }).then((res) => res.data);

export const cancelRequest = (requestId: number, reason?: string) =>
  api.post(`/api/esignature/requests/${requestId}/cancel`, { reason }).then((res) => res.data);

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
