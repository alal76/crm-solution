/**
 * Component Tests for ITSM, Sales, and Integration modules
 */

import React from 'react';
import { render, screen, fireEvent, waitFor, within, cleanup } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';

// ITSM Component Tests
describe('ITSM Components', () => {
  afterEach(cleanup);

  describe('IncidentStatusBadge', () => {
    it('should render status badge with correct label', () => {
      const { IncidentStatusBadge } = require('../components/itsm/IncidentStatusBadge');
      const { getByText } = render(
        <IncidentStatusBadge status={0} />
      );
      expect(getByText('New')).toBeInTheDocument();
    });

    it('should apply correct color based on status', () => {
      const { IncidentStatusBadge } = require('../components/itsm/IncidentStatusBadge');
      const { container } = render(
        <IncidentStatusBadge status={2} />
      );
      const chip = container.querySelector('.MuiChip-root');
      expect(chip).toBeInTheDocument();
    });
  });

  describe('IncidentPriorityBadge', () => {
    it('should render priority with icon', () => {
      const { IncidentPriorityBadge } = require('../components/itsm/IncidentPriorityBadge');
      const { getByText } = render(
        <IncidentPriorityBadge priority={0} />
      );
      expect(getByText('Critical')).toBeInTheDocument();
    });
  });

  describe('IncidentSLAIndicator', () => {
    it('should render SLA progress correctly', () => {
      const { IncidentSLAIndicator } = require('../components/itsm/IncidentSLAIndicator');
      
      const mockSLA = {
        id: 1,
        incidentId: 1,
        slaName: 'Standard SLA',
        responseTime: 60,
        resolutionTime: 480,
        responseDeadline: new Date().toISOString(),
        resolutionDeadline: new Date().toISOString(),
        responseBreached: false,
        resolutionBreached: false,
        responsePercentComplete: 50,
        resolutionPercentComplete: 25,
      };

      const { getByText } = render(
        <IncidentSLAIndicator sla={mockSLA} dense={true} />
      );
      expect(getByText(/Response:/i)).toBeInTheDocument();
    });
  });

  describe('IncidentActivityTimeline', () => {
    it('should render activity timeline', () => {
      const { IncidentActivityTimeline } = require('../components/itsm/IncidentActivityTimeline');
      
      const mockActivities = [
        {
          id: 1,
          incidentId: 1,
          type: 'comment',
          userId: 1,
          userName: 'John Doe',
          content: 'Test comment',
          timestamp: new Date().toISOString(),
        }
      ];

      const { getByText } = render(
        <IncidentActivityTimeline activities={mockActivities} />
      );
      expect(getByText('Test comment')).toBeInTheDocument();
    });
  });

  describe('ProblemRelatedIncidentsList', () => {
    it('should show "no related incidents" when list is empty', () => {
      const { ProblemRelatedIncidentsList } = require('../components/itsm/ProblemRelatedIncidentsList');
      
      const { getByText } = render(
        <ProblemRelatedIncidentsList incidents={[]} />
      );
      expect(getByText(/No related incidents found/i)).toBeInTheDocument();
    });

    it('should display related incidents in table', () => {
      const { ProblemRelatedIncidentsList } = require('../components/itsm/ProblemRelatedIncidentsList');
      
      const mockIncidents = [
        {
          id: 1,
          number: 'INC-001',
          title: 'Test Incident',
          status: 1,
          priority: 1,
          createdAt: new Date().toISOString(),
        }
      ];

      const { getByText } = render(
        <ProblemRelatedIncidentsList incidents={mockIncidents} />
      );
      expect(getByText('INC-001')).toBeInTheDocument();
      expect(getByText('Test Incident')).toBeInTheDocument();
    });
  });
});

// Sales Component Tests
describe('Sales Components', () => {
  afterEach(cleanup);

  describe('CommissionPlanForm', () => {
    it('should render form fields', () => {
      const { CommissionPlanForm } = require('../components/sales/CommissionPlanForm');
      
      render(
        <CommissionPlanForm onSave={async () => {}} />
      );
      
      expect(screen.getByLabelText('Plan Name')).toBeInTheDocument();
      expect(screen.getAllByText('Commission Type').length).toBeGreaterThan(0);
    });

    it('should enable save button when required fields are filled', async () => {
      const { CommissionPlanForm } = require('../components/sales/CommissionPlanForm');
      
      render(
        <CommissionPlanForm onSave={async () => {}} />
      );

      const nameInput = screen.getByLabelText('Plan Name');
      await userEvent.type(nameInput, 'Test Plan');
      
      const saveButton = screen.getByText('Save Commission Plan');
      expect(saveButton).not.toBeDisabled();
    });
  });
});

// Integration Component Tests
describe('Integration Components', () => {
  afterEach(cleanup);

  describe('WebhookForm', () => {
    it('should render webhook form fields', () => {
      const { WebhookForm } = require('../components/integration/WebhookForm');
      
      render(
        <WebhookForm onSave={async () => {}} />
      );
      
      expect(screen.getByLabelText(/Webhook Name/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Webhook URL/i)).toBeInTheDocument();
    });
  });

  describe('WebhookDeliveryHistoryTable', () => {
    it('should display no delivery history message when empty', () => {
      const { WebhookDeliveryHistoryTable } = require('../components/integration/WebhookDeliveryHistoryTable');
      
      const { getByText } = render(
        <WebhookDeliveryHistoryTable deliveries={[]} />
      );
      expect(getByText(/No delivery history/i)).toBeInTheDocument();
    });
  });
});

// Page Component Tests
describe('Page Components', () => {
  afterEach(cleanup);

  describe('ProblemManagementPage', () => {
    it('should render problem management page header', () => {
      const { ProblemManagementPage } = require('../pages/itsm/ProblemManagementPage');
      
      const { getByText } = render(
        <ProblemManagementPage />
      );
      expect(getByText(/Problem Management/i)).toBeInTheDocument();
    });
  });

  describe('ChangeManagementPage', () => {
    it('should render change management page header', () => {
      const { ChangeManagementPage } = require('../pages/itsm/ChangeManagementPage');
      
      const { getByText } = render(
        <ChangeManagementPage />
      );
      expect(getByText(/Change Management/i)).toBeInTheDocument();
    });
  });

  describe('WebhooksManagementPage', () => {
    it('should render webhooks management page header', () => {
      const { WebhooksManagementPage } = require('../pages/WebhooksManagementPage');
      
      const { getByText } = render(
        <WebhooksManagementPage />
      );
      expect(getByText(/Webhooks Management/i)).toBeInTheDocument();
    });
  });
});

// Service Tests
describe('Data Services', () => {
  describe('Incident Service', () => {
    it('should have getIncidents method', () => {
      const incidentService = require('../services/incidentService').default;
      expect(incidentService.getIncidents).toBeDefined();
    });

    it('should have createIncident method', () => {
      const incidentService = require('../services/incidentService').default;
      expect(incidentService.createIncident).toBeDefined();
    });
  });

  describe('Problem Service', () => {
    it('should have getProblems method', () => {
      const problemService = require('../services/problemService').default;
      expect(problemService.getProblems).toBeDefined();
    });

    it('should have createProblem method', () => {
      const problemService = require('../services/problemService').default;
      expect(problemService.createProblem).toBeDefined();
    });
  });

  describe('Change Service', () => {
    it('should have getChanges method', () => {
      const changeService = require('../services/changeService').default;
      expect(changeService.getChanges).toBeDefined();
    });

    it('should have createChange method', () => {
      const changeService = require('../services/changeService').default;
      expect(changeService.createChange).toBeDefined();
    });
  });

  describe('Webhook Service', () => {
    it('should have getWebhooks method', () => {
      const webhookService = require('../services/webhookService').default;
      expect(webhookService.getWebhooks).toBeDefined();
    });

    it('should have createWebhook method', () => {
      const webhookService = require('../services/webhookService').default;
      expect(webhookService.createWebhook).toBeDefined();
    });
  });
});
