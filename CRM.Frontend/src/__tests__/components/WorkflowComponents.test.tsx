import React from 'react';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../test-utils/renderWithProviders';
import WorkflowToolbar from '../../components/workflow/WorkflowToolbar';
import WorkflowList from '../../components/workflow/WorkflowList';
import type { WorkflowDefinition } from '../../services/workflowService';

describe('WorkflowToolbar', () => {
  it('renders error, success, and zoom percentage', () => {
    renderWithProviders(
      <WorkflowToolbar
        error="Failed to save"
        success="Saved"
        onClearError={jest.fn()}
        onClearSuccess={jest.fn()}
        zoom={1.25}
        onZoomOut={jest.fn()}
        onZoomIn={jest.fn()}
        onFitScreen={jest.fn()}
        showGrid
        onToggleGrid={jest.fn()}
        onOpenSimulator={jest.fn()}
        onOpenVersionHistory={jest.fn()}
        saving={false}
      />
    );

    expect(screen.getByText('Failed to save')).toBeInTheDocument();
    expect(screen.getByText('Saved')).toBeInTheDocument();
    expect(screen.getByText('125%')).toBeInTheDocument();
  });
});

describe('WorkflowList', () => {
  const baseWorkflow: WorkflowDefinition = {
    id: 1,
    workflowKey: 'WF-001',
    name: 'Incident Workflow',
    description: 'Handles incidents',
    category: 'ITSM',
    entityType: 'ServiceRequest',
    status: 'Active',
    currentVersion: 3,
    iconName: 'AccountTree',
    color: '#6750A4',
    isSystem: false,
    priority: 100,
    maxConcurrentInstances: 0,
    defaultTimeoutHours: 0,
    tags: [],
    createdAt: new Date().toISOString(),
  };

  it('renders empty state when no workflows exist', () => {
    renderWithProviders(
      <WorkflowList
        workflows={[]}
        loading={false}
        getStatusColor={() => '#000'}
        onOpenDesigner={jest.fn()}
        onViewInstances={jest.fn()}
        onActivate={jest.fn()}
        onPause={jest.fn()}
        onEdit={jest.fn()}
        onDelete={jest.fn()}
      />
    );

    expect(
      screen.getByText('No workflows found. Create your first workflow to get started.')
    ).toBeInTheDocument();
  });

  it('renders workflow rows with status', () => {
    renderWithProviders(
      <WorkflowList
        workflows={[baseWorkflow]}
        loading={false}
        getStatusColor={() => '#123456'}
        onOpenDesigner={jest.fn()}
        onViewInstances={jest.fn()}
        onActivate={jest.fn()}
        onPause={jest.fn()}
        onEdit={jest.fn()}
        onDelete={jest.fn()}
      />
    );

    expect(screen.getByText('Incident Workflow')).toBeInTheDocument();
    expect(screen.getByText('WF-001')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
  });
});
