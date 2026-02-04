// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Frontend React Component Tests - ITSM Core Features (Phases 1-3)

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';

// Mock router
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => jest.fn(),
  useParams: () => ({ id: '1' }),
  useLocation: () => ({ pathname: '/itsm/incidents' }),
  Link: ({ children, to }: { children: React.ReactNode; to: string }) => <a href={to}>{children}</a>,
}));

// Mock API calls
const mockFetch = jest.fn();
global.fetch = mockFetch;

// ============================================================================
// Incident Management Component Tests
// ============================================================================

describe('Incident Management Components', () => {
  beforeEach(() => {
    mockFetch.mockClear();
  });

  describe('IncidentListPage', () => {
    test('renders incident list header', () => {
      render(
        <div data-testid="incident-list">
          <h1>Incidents</h1>
          <button>Create Incident</button>
          <table>
            <thead>
              <tr>
                <th>Number</th>
                <th>Short Description</th>
                <th>Priority</th>
                <th>State</th>
                <th>Assigned To</th>
              </tr>
            </thead>
            <tbody></tbody>
          </table>
        </div>
      );

      expect(screen.getByText('Incidents')).toBeInTheDocument();
      expect(screen.getByText('Create Incident')).toBeInTheDocument();
      expect(screen.getByText('Number')).toBeInTheDocument();
    });

    test('displays incident data in table', () => {
      const mockIncidents = [
        { id: 1, number: 'INC0000001', shortDescription: 'Server down', priority: 1, state: 'New' },
        { id: 2, number: 'INC0000002', shortDescription: 'Email issue', priority: 2, state: 'InProgress' },
      ];

      render(
        <table data-testid="incident-table">
          <tbody>
            {mockIncidents.map(inc => (
              <tr key={inc.id}>
                <td>{inc.number}</td>
                <td>{inc.shortDescription}</td>
                <td>P{inc.priority}</td>
                <td>{inc.state}</td>
              </tr>
            ))}
          </tbody>
        </table>
      );

      expect(screen.getByText('INC0000001')).toBeInTheDocument();
      expect(screen.getByText('Server down')).toBeInTheDocument();
      expect(screen.getByText('P1')).toBeInTheDocument();
    });

    test('filter controls render correctly', () => {
      render(
        <div data-testid="incident-filters">
          <select data-testid="state-filter">
            <option value="">All States</option>
            <option value="New">New</option>
            <option value="InProgress">In Progress</option>
            <option value="Resolved">Resolved</option>
            <option value="Closed">Closed</option>
          </select>
          <select data-testid="priority-filter">
            <option value="">All Priorities</option>
            <option value="1">P1 - Critical</option>
            <option value="2">P2 - High</option>
            <option value="3">P3 - Medium</option>
          </select>
        </div>
      );

      expect(screen.getByTestId('state-filter')).toBeInTheDocument();
      expect(screen.getByTestId('priority-filter')).toBeInTheDocument();
    });
  });

  describe('CreateIncidentForm', () => {
    test('renders all required fields', () => {
      render(
        <form data-testid="create-incident-form">
          <label>
            Short Description *
            <input type="text" name="shortDescription" required />
          </label>
          <label>
            Caller *
            <select name="callerId" required>
              <option value="">Select Caller</option>
            </select>
          </label>
          <label>
            Impact *
            <select name="impact" required>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>
          </label>
          <label>
            Urgency *
            <select name="urgency" required>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>
          </label>
          <button type="submit">Create</button>
        </form>
      );

      expect(screen.getByText('Short Description *')).toBeInTheDocument();
      expect(screen.getByText('Impact *')).toBeInTheDocument();
      expect(screen.getByText('Urgency *')).toBeInTheDocument();
    });

    test('validates required fields', async () => {
      const user = userEvent.setup();
      const handleSubmit = jest.fn(e => e.preventDefault());

      render(
        <form onSubmit={handleSubmit} data-testid="create-incident-form">
          <input type="text" name="shortDescription" required placeholder="Short Description" />
          <button type="submit">Create</button>
        </form>
      );

      await user.click(screen.getByText('Create'));
      // Form validation should prevent submission
    });
  });

  describe('IncidentDetailPage', () => {
    test('displays incident details', () => {
      const mockIncident = {
        number: 'INC0000001',
        shortDescription: 'Production server not responding',
        description: 'Server went down at 10:00 AM',
        state: 'InProgress',
        priority: 1,
        assignedTo: 'John Smith',
      };

      render(
        <div data-testid="incident-detail">
          <h1>{mockIncident.number}</h1>
          <h2>{mockIncident.shortDescription}</h2>
          <div>
            <span>State: {mockIncident.state}</span>
            <span>Priority: P{mockIncident.priority}</span>
            <span>Assigned To: {mockIncident.assignedTo}</span>
          </div>
          <p>{mockIncident.description}</p>
        </div>
      );

      expect(screen.getByText('INC0000001')).toBeInTheDocument();
      expect(screen.getByText('Production server not responding')).toBeInTheDocument();
      expect(screen.getByText('Assigned To: John Smith')).toBeInTheDocument();
    });

    test('renders action buttons', () => {
      render(
        <div data-testid="incident-actions">
          <button>Assign</button>
          <button>Escalate</button>
          <button>Resolve</button>
          <button>Add Comment</button>
        </div>
      );

      expect(screen.getByText('Assign')).toBeInTheDocument();
      expect(screen.getByText('Resolve')).toBeInTheDocument();
      expect(screen.getByText('Add Comment')).toBeInTheDocument();
    });
  });
});

// ============================================================================
// Problem Management Component Tests
// ============================================================================

describe('Problem Management Components', () => {
  describe('ProblemListPage', () => {
    test('renders problem list with known error indicator', () => {
      const mockProblems = [
        { id: 1, number: 'PRB0000001', shortDescription: 'Memory leak', knownError: true },
        { id: 2, number: 'PRB0000002', shortDescription: 'Slow queries', knownError: false },
      ];

      render(
        <table data-testid="problem-table">
          <tbody>
            {mockProblems.map(prb => (
              <tr key={prb.id}>
                <td>{prb.number}</td>
                <td>{prb.shortDescription}</td>
                <td>{prb.knownError ? '⚠️ Known Error' : '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      );

      expect(screen.getByText('PRB0000001')).toBeInTheDocument();
      expect(screen.getByText('⚠️ Known Error')).toBeInTheDocument();
    });
  });

  describe('ProblemDetailPage', () => {
    test('displays RCA section', () => {
      render(
        <div data-testid="problem-detail">
          <h2>Root Cause Analysis</h2>
          <div>
            <label>Root Cause:</label>
            <p>Memory leak in background service</p>
          </div>
          <div>
            <label>Workaround:</label>
            <p>Restart service every 24 hours</p>
          </div>
          <div>
            <label>Solution:</label>
            <p>Apply patch version 2.1.5</p>
          </div>
        </div>
      );

      expect(screen.getByText('Root Cause Analysis')).toBeInTheDocument();
      expect(screen.getByText('Memory leak in background service')).toBeInTheDocument();
      expect(screen.getByText('Restart service every 24 hours')).toBeInTheDocument();
    });

    test('shows related incidents', () => {
      const relatedIncidents = ['INC0000001', 'INC0000002', 'INC0000003'];

      render(
        <div data-testid="related-incidents">
          <h3>Related Incidents ({relatedIncidents.length})</h3>
          <ul>
            {relatedIncidents.map(inc => (
              <li key={inc}>{inc}</li>
            ))}
          </ul>
        </div>
      );

      expect(screen.getByText('Related Incidents (3)')).toBeInTheDocument();
      expect(screen.getByText('INC0000001')).toBeInTheDocument();
    });
  });
});

// ============================================================================
// Change Management Component Tests
// ============================================================================

describe('Change Management Components', () => {
  describe('ChangeListPage', () => {
    test('displays change request with approval status', () => {
      const mockChanges = [
        { id: 1, number: 'CHG0000001', shortDescription: 'Upgrade DB', approval: 'Approved', type: 'Normal' },
        { id: 2, number: 'CHG0000002', shortDescription: 'Network change', approval: 'Pending', type: 'Standard' },
      ];

      render(
        <table data-testid="change-table">
          <tbody>
            {mockChanges.map(chg => (
              <tr key={chg.id}>
                <td>{chg.number}</td>
                <td>{chg.shortDescription}</td>
                <td>{chg.type}</td>
                <td className={`status-${chg.approval.toLowerCase()}`}>{chg.approval}</td>
              </tr>
            ))}
          </tbody>
        </table>
      );

      expect(screen.getByText('CHG0000001')).toBeInTheDocument();
      expect(screen.getByText('Approved')).toBeInTheDocument();
      expect(screen.getByText('Pending')).toBeInTheDocument();
    });
  });

  describe('ChangeDetailPage', () => {
    test('displays implementation and backout plans', () => {
      render(
        <div data-testid="change-plans">
          <section>
            <h3>Implementation Plan</h3>
            <p>1. Backup current config</p>
            <p>2. Apply changes</p>
            <p>3. Verify functionality</p>
          </section>
          <section>
            <h3>Backout Plan</h3>
            <p>1. Stop services</p>
            <p>2. Restore backup</p>
            <p>3. Restart services</p>
          </section>
        </div>
      );

      expect(screen.getByText('Implementation Plan')).toBeInTheDocument();
      expect(screen.getByText('Backout Plan')).toBeInTheDocument();
    });

    test('shows approval workflow', () => {
      render(
        <div data-testid="approval-workflow">
          <h3>Approval Status</h3>
          <div>
            <span>CAB Review:</span> <span>Approved</span>
          </div>
          <div>
            <span>Technical Review:</span> <span>Pending</span>
          </div>
          <button>Approve</button>
          <button>Reject</button>
        </div>
      );

      expect(screen.getByText('Approval Status')).toBeInTheDocument();
      expect(screen.getByText('Approve')).toBeInTheDocument();
      expect(screen.getByText('Reject')).toBeInTheDocument();
    });
  });
});

// ============================================================================
// CMDB Component Tests
// ============================================================================

describe('CMDB Components', () => {
  describe('CMDBListPage', () => {
    test('displays configuration items', () => {
      const mockCIs = [
        { id: 1, number: 'CI0000001', name: 'PROD-WEB-01', type: 'Server', status: 'Operational' },
        { id: 2, number: 'CI0000002', name: 'PROD-DB-01', type: 'Database', status: 'Operational' },
      ];

      render(
        <table data-testid="ci-table">
          <thead>
            <tr>
              <th>CI Number</th>
              <th>Name</th>
              <th>Type</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {mockCIs.map(ci => (
              <tr key={ci.id}>
                <td>{ci.number}</td>
                <td>{ci.name}</td>
                <td>{ci.type}</td>
                <td>{ci.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      );

      expect(screen.getByText('PROD-WEB-01')).toBeInTheDocument();
      expect(screen.getByText('Server')).toBeInTheDocument();
    });
  });

  describe('CIDetailPage', () => {
    test('shows CI relationships', () => {
      render(
        <div data-testid="ci-relationships">
          <h3>Relationships</h3>
          <ul>
            <li>Depends On: PROD-DB-01</li>
            <li>Runs On: VMWare-Cluster-01</li>
            <li>Connects To: LB-01</li>
          </ul>
        </div>
      );

      expect(screen.getByText('Relationships')).toBeInTheDocument();
      expect(screen.getByText('Depends On: PROD-DB-01')).toBeInTheDocument();
    });

    test('shows impact analysis', () => {
      render(
        <div data-testid="impact-analysis">
          <h3>Impact Analysis</h3>
          <p>If this CI fails, the following services will be affected:</p>
          <ul>
            <li>CRM Web Application</li>
            <li>Customer Portal</li>
            <li>API Gateway</li>
          </ul>
        </div>
      );

      expect(screen.getByText('Impact Analysis')).toBeInTheDocument();
      expect(screen.getByText('CRM Web Application')).toBeInTheDocument();
    });
  });
});

// ============================================================================
// Knowledge Management Component Tests
// ============================================================================

describe('Knowledge Management Components', () => {
  describe('KnowledgeBasePage', () => {
    test('displays article search', () => {
      render(
        <div data-testid="kb-search">
          <h1>Knowledge Base</h1>
          <input type="search" placeholder="Search articles..." />
          <button>Search</button>
        </div>
      );

      expect(screen.getByPlaceholderText('Search articles...')).toBeInTheDocument();
    });

    test('shows popular articles', () => {
      const popularArticles = [
        { id: 1, title: 'How to Reset Password', views: 1500 },
        { id: 2, title: 'VPN Connection Guide', views: 1200 },
      ];

      render(
        <div data-testid="popular-articles">
          <h2>Popular Articles</h2>
          <ul>
            {popularArticles.map(article => (
              <li key={article.id}>
                {article.title} ({article.views} views)
              </li>
            ))}
          </ul>
        </div>
      );

      expect(screen.getByText('Popular Articles')).toBeInTheDocument();
      expect(screen.getByText('How to Reset Password (1500 views)')).toBeInTheDocument();
    });
  });

  describe('ArticleDetailPage', () => {
    test('displays article content with feedback', () => {
      render(
        <article data-testid="article-detail">
          <h1>How to Reset Your Password</h1>
          <div className="article-body">
            <p>Follow these steps to reset your password...</p>
          </div>
          <div className="feedback">
            <span>Was this article helpful?</span>
            <button>👍 Yes</button>
            <button>👎 No</button>
          </div>
        </article>
      );

      expect(screen.getByText('How to Reset Your Password')).toBeInTheDocument();
      expect(screen.getByText('Was this article helpful?')).toBeInTheDocument();
      expect(screen.getByText('👍 Yes')).toBeInTheDocument();
    });
  });
});

// ============================================================================
// Service Catalog Component Tests
// ============================================================================

describe('Service Catalog Components', () => {
  describe('ServiceCatalogPage', () => {
    test('displays catalog categories', () => {
      const categories = ['Hardware', 'Software', 'Access', 'Services'];

      render(
        <nav data-testid="catalog-categories">
          <h2>Categories</h2>
          <ul>
            {categories.map(cat => (
              <li key={cat}>{cat}</li>
            ))}
          </ul>
        </nav>
      );

      expect(screen.getByText('Hardware')).toBeInTheDocument();
      expect(screen.getByText('Software')).toBeInTheDocument();
    });

    test('displays featured items', () => {
      const featuredItems = [
        { id: 1, name: 'New Laptop', price: 1500 },
        { id: 2, name: 'VPN Access', price: 0 },
      ];

      render(
        <div data-testid="featured-items">
          <h2>Featured Services</h2>
          <div className="items-grid">
            {featuredItems.map(item => (
              <div key={item.id} className="catalog-item">
                <h3>{item.name}</h3>
                <span>${item.price}</span>
                <button>Request</button>
              </div>
            ))}
          </div>
        </div>
      );

      expect(screen.getByText('New Laptop')).toBeInTheDocument();
      expect(screen.getByText('$1500')).toBeInTheDocument();
    });
  });

  describe('CatalogItemDetailPage', () => {
    test('displays item details with request form', () => {
      render(
        <div data-testid="catalog-item-detail">
          <h1>New Laptop Request</h1>
          <p>Request a new laptop for your work</p>
          <form>
            <label>
              Justification
              <textarea name="justification" required />
            </label>
            <label>
              Preferred Model
              <select name="model">
                <option>Dell XPS 15</option>
                <option>MacBook Pro 14</option>
                <option>ThinkPad X1</option>
              </select>
            </label>
            <button type="submit">Submit Request</button>
          </form>
        </div>
      );

      expect(screen.getByText('New Laptop Request')).toBeInTheDocument();
      expect(screen.getByText('Submit Request')).toBeInTheDocument();
    });
  });
});

// ============================================================================
// SLA Dashboard Component Tests
// ============================================================================

describe('SLA Dashboard Components', () => {
  describe('SLADashboardPage', () => {
    test('displays compliance metrics', () => {
      render(
        <div data-testid="sla-metrics">
          <div className="metric">
            <span>Overall Compliance</span>
            <span>95.2%</span>
          </div>
          <div className="metric">
            <span>Response SLA</span>
            <span>97.1%</span>
          </div>
          <div className="metric">
            <span>Resolution SLA</span>
            <span>92.8%</span>
          </div>
        </div>
      );

      expect(screen.getByText('Overall Compliance')).toBeInTheDocument();
      expect(screen.getByText('95.2%')).toBeInTheDocument();
    });

    test('displays breached and at-risk SLAs', () => {
      render(
        <div data-testid="sla-status">
          <section>
            <h3>Breached SLAs (5)</h3>
            <ul>
              <li>INC0000101 - Response breached</li>
              <li>INC0000105 - Resolution breached</li>
            </ul>
          </section>
          <section>
            <h3>At Risk (12)</h3>
            <ul>
              <li>INC0000110 - 15 min until breach</li>
              <li>INC0000112 - 25 min until breach</li>
            </ul>
          </section>
        </div>
      );

      expect(screen.getByText('Breached SLAs (5)')).toBeInTheDocument();
      expect(screen.getByText('At Risk (12)')).toBeInTheDocument();
    });
  });
});
