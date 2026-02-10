import api from 'src/services/api';
import type { EmailSequence } from 'src/types/api';

type Params = Record<string, any>;

export const getEmailSequences = (params?: Params): Promise<EmailSequence[]> =>
  api.get('/api/emailsequences', { params }).then((res) => res.data);

export const getEmailSequence = (id: number): Promise<EmailSequence> =>
  api.get(`/api/emailsequences/${id}`).then((res) => res.data);

export const createEmailSequence = (payload: Partial<EmailSequence>): Promise<EmailSequence> =>
  api.post('/api/emailsequences', payload).then((res) => res.data);

export const updateEmailSequence = (id: number, payload: Partial<EmailSequence>): Promise<EmailSequence> =>
  api.put(`/api/emailsequences/${id}`, payload).then((res) => res.data);

export const deleteEmailSequence = (id: number): Promise<void> =>
  api.delete(`/api/emailsequences/${id}`).then((res) => res.data);

export const enrollContact = (sequenceId: number, contactId: number, options?: Record<string, unknown>) =>
  api.post(`/api/emailsequences/${sequenceId}/enroll`, { contactId, ...options }).then((res) => res.data);

export const previewSequence = (sequenceId: number, data?: Record<string, unknown>) =>
  api.post(`/api/emailsequences/${sequenceId}/preview`, data).then((res) => res.data);

export const sendTest = (sequenceId: number, recipientEmail: string, testData?: Record<string, unknown>) =>
  api.post(`/api/emailsequences/${sequenceId}/send-test`, { recipientEmail, testData }).then((res) => res.data);

export const getSequenceStats = (sequenceId: number) =>
  api.get(`/api/emailsequences/${sequenceId}/stats`).then((res) => res.data);

export default {
  getEmailSequences,
  getEmailSequence,
  createEmailSequence,
  updateEmailSequence,
  deleteEmailSequence,
  enrollContact,
  previewSequence,
  sendTest,
  getSequenceStats,
};
