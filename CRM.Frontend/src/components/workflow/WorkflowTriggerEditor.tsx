/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Trigger Editor - wrapper for trigger configuration panel
 */

import React from 'react';
import { TriggerPropertiesPanel, type TriggerConfiguration } from './TriggerPropertiesPanel';

interface WorkflowTriggerEditorProps {
  value: TriggerConfiguration;
  onChange: (value: TriggerConfiguration) => void;
  entityType?: string;
  readonly?: boolean;
}

const WorkflowTriggerEditor: React.FC<WorkflowTriggerEditorProps> = (props) => {
  return <TriggerPropertiesPanel {...props} />;
};

export default WorkflowTriggerEditor;
