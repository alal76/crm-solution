/**
 * CRM Solution - Workflow Task Service
 * Convenience wrapper around workflowInstanceService task APIs.
 */

import { workflowInstanceService } from './workflow';
import type { HumanTask } from './workflow';

export const workflowTaskService = {
  async getMyTasks(): Promise<HumanTask[]> {
    return workflowInstanceService.getMyTasks();
  },
  async claimTask(taskId: number): Promise<void> {
    return workflowInstanceService.claimTask(taskId);
  },
  async completeTask(taskId: number, formData?: string, outputData?: string): Promise<void> {
    return workflowInstanceService.completeTask(taskId, formData, outputData);
  }
};

export default workflowTaskService;
