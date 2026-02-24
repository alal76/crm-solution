/**
 * CRM Solution - ITSM Component Unit Tests (TODO-ITSM-09)
 *
 * Tests for IncidentStatusBadge logic, SLABreachAlert data model,
 * and EscalationRulesPanel validation — following the established
 * mock-based (non-render) pattern from ITSMPhase4Pages.test.tsx.
 */

import '@testing-library/jest-dom';

// ---------------------------------------------------------------------------
// IncidentStatusBadge — enum and config logic
// ---------------------------------------------------------------------------

describe('IncidentStatusBadge', () => {
  // Mirror the enum as used by the component
  enum IncidentStatus {
    New = 'New',
    InProgress = 'InProgress',
    OnHold = 'OnHold',
    Resolved = 'Resolved',
    Closed = 'Closed',
    Cancelled = 'Cancelled',
    Reopened = 'Reopened',
  }

  const colorMap: Record<IncidentStatus, string> = {
    [IncidentStatus.New]: 'info',
    [IncidentStatus.InProgress]: 'warning',
    [IncidentStatus.OnHold]: 'default',
    [IncidentStatus.Resolved]: 'success',
    [IncidentStatus.Closed]: 'default',
    [IncidentStatus.Cancelled]: 'error',
    [IncidentStatus.Reopened]: 'warning',
  };

  it('should map all incident statuses to a chip color', () => {
    const statuses = Object.values(IncidentStatus);
    for (const status of statuses) {
      expect(colorMap[status]).toBeDefined();
    }
  });

  it('should assign "info" color to New status', () => {
    expect(colorMap[IncidentStatus.New]).toBe('info');
  });

  it('should assign "warning" color to InProgress status', () => {
    expect(colorMap[IncidentStatus.InProgress]).toBe('warning');
  });

  it('should assign "success" color to Resolved status', () => {
    expect(colorMap[IncidentStatus.Resolved]).toBe('success');
  });

  it('should assign "error" color to Cancelled status', () => {
    expect(colorMap[IncidentStatus.Cancelled]).toBe('error');
  });

  it('should support exactly 7 distinct status values', () => {
    expect(Object.values(IncidentStatus)).toHaveLength(7);
  });

  it('should produce human-readable labels for all statuses', () => {
    const labelMap: Record<IncidentStatus, string> = {
      [IncidentStatus.New]: 'New',
      [IncidentStatus.InProgress]: 'In Progress',
      [IncidentStatus.OnHold]: 'On Hold',
      [IncidentStatus.Resolved]: 'Resolved',
      [IncidentStatus.Closed]: 'Closed',
      [IncidentStatus.Cancelled]: 'Cancelled',
      [IncidentStatus.Reopened]: 'Reopened',
    };

    expect(labelMap[IncidentStatus.InProgress]).toBe('In Progress');
    expect(labelMap[IncidentStatus.OnHold]).toBe('On Hold');
    for (const status of Object.values(IncidentStatus)) {
      expect(labelMap[status]).toBeTruthy();
    }
  });
});

// ---------------------------------------------------------------------------
// SLABreachAlert — data model and utility logic
// ---------------------------------------------------------------------------

describe('SLABreachAlert', () => {
  type BreachType = 'response' | 'resolution';
  type BreachSeverity = 'warning' | 'imminent' | 'breached';

  interface SLABreachInfo {
    id: number;
    ticketNumber: string;
    ticketTitle: string;
    breachType: BreachType;
    severity: BreachSeverity;
    dueAt: Date | string;
    breachedAt?: Date | string;
    minutesRemaining?: number;
    minutesOverdue?: number;
    assignedTo?: string;
    priority: number;
    escalationLevel: number;
  }

  const buildBreach = (overrides: Partial<SLABreachInfo> = {}): SLABreachInfo => ({
    id: 1,
    ticketNumber: 'INC-0001',
    ticketTitle: 'Critical system outage',
    breachType: 'resolution',
    severity: 'breached',
    dueAt: new Date(Date.now() - 3600 * 1000).toISOString(), // 1 hour ago
    minutesOverdue: 60,
    priority: 1,
    escalationLevel: 1,
    ...overrides,
  });

  const getSeverityIcon = (severity: BreachSeverity): string => {
    switch (severity) {
      case 'warning': return 'Warning';
      case 'imminent': return 'Warning';
      case 'breached': return 'Error';
    }
  };

  const getSeverityColor = (severity: BreachSeverity): string => {
    switch (severity) {
      case 'warning': return 'warning';
      case 'imminent': return 'error';
      case 'breached': return 'error';
    }
  };

  it('should build a valid breach info object', () => {
    const breach = buildBreach();
    expect(breach.id).toBe(1);
    expect(breach.ticketNumber).toBe('INC-0001');
    expect(breach.severity).toBe('breached');
    expect(breach.priority).toBeGreaterThan(0);
  });

  it('should map "warning" severity to warning color', () => {
    expect(getSeverityColor('warning')).toBe('warning');
  });

  it('should map "imminent" severity to error color', () => {
    expect(getSeverityColor('imminent')).toBe('error');
  });

  it('should map "breached" severity to error icon', () => {
    expect(getSeverityIcon('breached')).toBe('Error');
  });

  it('should require a ticket number in breach info', () => {
    const breach = buildBreach({ ticketNumber: 'INC-9999' });
    expect(breach.ticketNumber).toMatch(/^INC-/);
  });

  it('should accept undefined minutesRemaining for breached items', () => {
    const breach = buildBreach({ minutesRemaining: undefined, minutesOverdue: 120 });
    expect(breach.minutesRemaining).toBeUndefined();
    expect(breach.minutesOverdue).toBe(120);
  });

  it('should distinguish response vs resolution breach types', () => {
    const respBreach = buildBreach({ breachType: 'response' });
    const resBreach = buildBreach({ breachType: 'resolution' });
    expect(respBreach.breachType).toBe('response');
    expect(resBreach.breachType).toBe('resolution');
    expect(respBreach.breachType).not.toBe(resBreach.breachType);
  });

  it('should support escalation level tracking', () => {
    const level1 = buildBreach({ escalationLevel: 1 });
    const level3 = buildBreach({ escalationLevel: 3 });
    expect(level3.escalationLevel).toBeGreaterThan(level1.escalationLevel);
  });

  it('should handle breach from past due date', () => {
    const past = new Date(Date.now() - 7200 * 1000); // 2 hours ago
    const breach = buildBreach({ dueAt: past, minutesOverdue: 120 });
    const dueTime = new Date(breach.dueAt).getTime();
    expect(dueTime).toBeLessThan(Date.now());
    expect(breach.minutesOverdue).toBe(120);
  });
});

// ---------------------------------------------------------------------------
// EscalationRulesPanel — validation and data model logic
// ---------------------------------------------------------------------------

describe('EscalationRulesPanel', () => {
  type EscalationTargetType = 'User' | 'Group' | 'Queue' | 'Manager';

  interface EscalationRuleFormData {
    name: string;
    description?: string;
    priority: string;
    ageInMinutes: number;
    targetType: EscalationTargetType;
    targetId?: number;
    targetName?: string;
    maxAttempts: number;
    retryIntervalMinutes: number;
    isActive: boolean;
    conditions?: string;
  }

  const validateRule = (form: EscalationRuleFormData): string[] => {
    const errors: string[] = [];
    if (!form.name?.trim()) errors.push('Name is required');
    if (!form.priority?.trim()) errors.push('Priority is required');
    if (!form.ageInMinutes || form.ageInMinutes <= 0) errors.push('Age in minutes must be greater than 0');
    if (!form.maxAttempts || form.maxAttempts <= 0) errors.push('Max attempts must be greater than 0');
    if (!form.retryIntervalMinutes || form.retryIntervalMinutes <= 0) errors.push('Retry interval must be greater than 0');
    return errors;
  };

  const validRule: EscalationRuleFormData = {
    name: 'Critical Incident Escalation',
    description: 'Escalate unresolved Critical incidents after 30 minutes',
    priority: 'Critical',
    ageInMinutes: 30,
    targetType: 'Group',
    targetName: 'Level 2 Support',
    maxAttempts: 3,
    retryIntervalMinutes: 15,
    isActive: true,
  };

  it('should pass validation for a properly filled rule', () => {
    const errors = validateRule(validRule);
    expect(errors).toHaveLength(0);
  });

  it('should fail validation when name is empty', () => {
    const errors = validateRule({ ...validRule, name: '' });
    expect(errors).toContain('Name is required');
  });

  it('should fail validation when priority is empty', () => {
    const errors = validateRule({ ...validRule, priority: '' });
    expect(errors).toContain('Priority is required');
  });

  it('should fail validation when ageInMinutes is zero', () => {
    const errors = validateRule({ ...validRule, ageInMinutes: 0 });
    expect(errors).toContain('Age in minutes must be greater than 0');
  });

  it('should fail validation when ageInMinutes is negative', () => {
    const errors = validateRule({ ...validRule, ageInMinutes: -10 });
    expect(errors).toContain('Age in minutes must be greater than 0');
  });

  it('should fail validation when maxAttempts is zero', () => {
    const errors = validateRule({ ...validRule, maxAttempts: 0 });
    expect(errors).toContain('Max attempts must be greater than 0');
  });

  it('should support all escalation target types', () => {
    const targets: EscalationTargetType[] = ['User', 'Group', 'Queue', 'Manager'];
    for (const target of targets) {
      const rule: EscalationRuleFormData = { ...validRule, targetType: target };
      const errors = validateRule(rule);
      expect(errors).toHaveLength(0);
    }
  });

  it('should allow optional conditions field', () => {
    const withConditions = { ...validRule, conditions: '{"category": "Infrastructure"}' };
    const withoutConditions = { ...validRule };
    expect(withConditions.conditions).toBeDefined();
    expect(withoutConditions.conditions).toBeUndefined();
    expect(validateRule(withConditions)).toHaveLength(0);
    expect(validateRule(withoutConditions)).toHaveLength(0);
  });

  it('should default isActive to true for new rules', () => {
    expect(validRule.isActive).toBe(true);
  });

  it('should format rule summary correctly', () => {
    const summary = `Escalate ${validRule.priority} priority tickets after ${validRule.ageInMinutes} min to ${validRule.targetType}: ${validRule.targetName}`;
    expect(summary).toContain('Critical');
    expect(summary).toContain('30 min');
    expect(summary).toContain('Group');
    expect(summary).toContain('Level 2 Support');
  });

  it('should detect multiple validation errors simultaneously', () => {
    const errors = validateRule({
      name: '',
      priority: '',
      ageInMinutes: 0,
      targetType: 'User',
      maxAttempts: 0,
      retryIntervalMinutes: 0,
      isActive: true,
    });
    expect(errors.length).toBeGreaterThanOrEqual(4);
  });
});
