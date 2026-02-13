/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Action Config Panel - wrapper for action configuration
 */

import React from 'react';
import { ActionPropertiesPanel, type ActionConfiguration } from './ActionPropertiesPanel';

interface ActionConfigPanelProps {
  value: ActionConfiguration;
  onChange: (value: ActionConfiguration) => void;
  readonly?: boolean;
}

const ActionConfigPanel: React.FC<ActionConfigPanelProps> = (props) => {
  return <ActionPropertiesPanel {...props} />;
};

export default ActionConfigPanel;
