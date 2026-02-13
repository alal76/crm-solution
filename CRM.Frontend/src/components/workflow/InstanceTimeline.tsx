/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Instance Timeline - wrapper around ExecutionTimeline for workflow instance history
 */

import React from 'react';
import { ExecutionTimeline, type TimelineStep } from './ExecutionTimeline';

interface InstanceTimelineProps {
  steps: TimelineStep[];
  workflowStartedAt?: string;
  workflowCompletedAt?: string;
  showDurations?: boolean;
}

const InstanceTimeline: React.FC<InstanceTimelineProps> = (props) => {
  return <ExecutionTimeline {...props} />;
};

export default InstanceTimeline;
