import api from './apiClient';
import type { EmailSequence } from '../types/api';

type Params = Record<string, any>;

export const getEmailSequences = (params?: Params): Promise<EmailSequence[]> =>
  api.get('/emailsequences', { params }).then((res) => res.data);

export const getEmailSequence = (id: number): Promise<EmailSequence> =>
  api.get(`/emailsequences/${id}`).then((res) => res.data);

export const createEmailSequence = (payload: Partial<EmailSequence>): Promise<EmailSequence> =>
  api.post('/emailsequences', payload).then((res) => res.data);

export const updateEmailSequence = (id: number, payload: Partial<EmailSequence>): Promise<EmailSequence> =>
  api.put(`/emailsequences/${id}`, payload).then((res) => res.data);

export const deleteEmailSequence = (id: number): Promise<void> =>
  api.delete(`/emailsequences/${id}`).then((res) => res.data);

export const enrollContact = (sequenceId: number, contactId: number, options?: Record<string, unknown>) =>
  api.post(`/emailsequences/${sequenceId}/enroll`, { contactId, ...options }).then((res) => res.data);

export const previewSequence = (sequenceId: number, data?: Record<string, unknown>) =>
  api.post(`/emailsequences/${sequenceId}/preview`, data).then((res) => res.data);

export const sendTest = (sequenceId: number, recipientEmail: string, testData?: Record<string, unknown>) =>
  api.post(`/emailsequences/${sequenceId}/send-test`, { recipientEmail, testData }).then((res) => res.data);

export const getSequenceStats = (sequenceId: number) =>
  api.get(`/emailsequences/${sequenceId}/stats`).then((res) => res.data);

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
