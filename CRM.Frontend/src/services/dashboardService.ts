/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import apiClient from './apiClient';

// Enums
export enum WidgetType {
  StatCard = 0,
  LineChart = 1,
  BarChart = 2,
  PieChart = 3,
  DataTable = 4,
  ActivityList = 5,
  TaskList = 6,
  PipelineFunnel = 7,
  ProgressGauge = 8,
  MapWidget = 9,
  CalendarWidget = 10,
  CustomContent = 11,
  Leaderboard = 12,
  KPICard = 13,
  AreaChart = 14,
  StackedBarChart = 15
}

export const widgetTypeLabels: Record<WidgetType, string> = {
  [WidgetType.StatCard]: 'Stat Card',
  [WidgetType.LineChart]: 'Line Chart',
  [WidgetType.BarChart]: 'Bar Chart',
  [WidgetType.PieChart]: 'Pie Chart',
  [WidgetType.DataTable]: 'Data Table',
  [WidgetType.ActivityList]: 'Activity List',
  [WidgetType.TaskList]: 'Task List',
  [WidgetType.PipelineFunnel]: 'Pipeline Funnel',
  [WidgetType.ProgressGauge]: 'Progress Gauge',
  [WidgetType.MapWidget]: 'Map',
  [WidgetType.CalendarWidget]: 'Calendar',
  [WidgetType.CustomContent]: 'Custom Content',
  [WidgetType.Leaderboard]: 'Leaderboard',
  [WidgetType.KPICard]: 'KPI Card',
  [WidgetType.AreaChart]: 'Area Chart',
  [WidgetType.StackedBarChart]: 'Stacked Bar Chart'
};

export enum DashboardVisibility {
  Public = 'Public',
  Private = 'Private',
  RoleBased = 'RoleBased'
}

// Types
export interface Dashboard {
  id: number;
  name: string;
  description?: string;
  isDefault: boolean;
  isSystem: boolean;
  isActive: boolean;
  iconName: string;
  displayOrder: number;
  columnCount: number;
  refreshIntervalSeconds: number;
  visibility: string;
  allowedRoles?: string;
  ownerId?: number;
  ownerName?: string;
  widgetCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface DashboardDetail extends Dashboard {
  layoutConfig?: string;
  widgets: DashboardWidget[];
}

export interface DashboardWidget {
  id: number;
  title: string;
  subtitle?: string;
  widgetType: string;
  widgetTypeValue: number;
  dataSource: string;
  rowIndex: number;
  columnIndex: number;
  columnSpan: number;
  rowSpan: number;
  displayOrder: number;
  isVisible: boolean;
  iconName?: string;
  color?: string;
  backgroundColor?: string;
  navigationLink?: string;
  configJson?: string;
  showTrend: boolean;
  trendPeriodDays: number;
  refreshIntervalSeconds: number;
}

export interface CreateDashboard {
  name: string;
  description?: string;
  isDefault?: boolean;
  isActive?: boolean;
  iconName?: string;
  displayOrder?: number;
  columnCount?: number;
  refreshIntervalSeconds?: number;
  layoutConfig?: string;
  ownerId?: number;
  visibility?: string;
  allowedRoles?: string;
}

export interface UpdateDashboard {
  name?: string;
  description?: string;
  isDefault?: boolean;
  isActive?: boolean;
  iconName?: string;
  displayOrder?: number;
  columnCount?: number;
  refreshIntervalSeconds?: number;
  layoutConfig?: string;
  visibility?: string;
  allowedRoles?: string;
}

export interface CreateWidget {
  dashboardId: number;
  title: string;
  subtitle?: string;
  widgetType?: string;
  dataSource?: string;
  rowIndex?: number;
  columnIndex?: number;
  columnSpan?: number;
  rowSpan?: number;
  displayOrder?: number;
  isVisible?: boolean;
  iconName?: string;
  color?: string;
  backgroundColor?: string;
  navigationLink?: string;
  configJson?: string;
  showTrend?: boolean;
  trendPeriodDays?: number;
  refreshIntervalSeconds?: number;
}

export interface UpdateWidget {
  title?: string;
  subtitle?: string;
  widgetType?: string;
  dataSource?: string;
  rowIndex?: number;
  columnIndex?: number;
  columnSpan?: number;
  rowSpan?: number;
  displayOrder?: number;
  isVisible?: boolean;
  iconName?: string;
  color?: string;
  backgroundColor?: string;
  navigationLink?: string;
  configJson?: string;
  showTrend?: boolean;
  trendPeriodDays?: number;
  refreshIntervalSeconds?: number;
}

export interface WidgetOrder {
  widgetId: number;
  displayOrder: number;
  rowIndex?: number;
  columnIndex?: number;
}

export interface DataSource {
  id: string;
  name: string;
  category: string;
  type: string;
}

export interface WidgetTypeOption {
  value: number;
  name: string;
}

// Dashboard Stats (from dashboard controller)
export interface DashboardStats {
  customers: { total: number };
  contacts: { total: number };
  opportunities: { total: number; openValue: number; wonValue: number };
  products: { total: number };
  tasks: { total: number; pending: number };
  users: { active: number };
  timestamp: string;
}

export interface PipelineStage {
  stage: string;
  stageValue: number;
  count: number;
  totalValue: number;
  weightedValue: number;
}

export interface PipelineSummary {
  stages: PipelineStage[];
  summary: {
    totalValue: number;
    weightedValue: number;
    opportunityCount: number;
  };
}

export interface RecentActivity {
  id: number;
  type: string;
  title: string;
  activityDate: string;
  description?: string;
  entityType?: string;
  entityId?: number;
}

// Dashboard Configuration Service
export const dashboardConfigService = {
  // Dashboard CRUD
  getDashboards: () => apiClient.get<Dashboard[]>('/dashboard-config/dashboards'),
  getAllDashboards: () => apiClient.get<Dashboard[]>('/dashboard-config/dashboards/all'),
  getDashboard: (id: number) => apiClient.get<DashboardDetail>(`/dashboard-config/dashboards/${id}`),
  getDefaultDashboard: () => apiClient.get<DashboardDetail>('/dashboard-config/dashboards/default'),
  createDashboard: (data: CreateDashboard) => apiClient.post<{ id: number; name: string }>('/dashboard-config/dashboards', data),
  updateDashboard: (id: number, data: UpdateDashboard) => apiClient.put(`/dashboard-config/dashboards/${id}`, data),
  deleteDashboard: (id: number) => apiClient.delete(`/dashboard-config/dashboards/${id}`),

  // Widget CRUD
  getWidgets: (dashboardId: number) => apiClient.get<DashboardWidget[]>(`/dashboard-config/dashboards/${dashboardId}/widgets`),
  createWidget: (data: CreateWidget) => apiClient.post<{ id: number; title: string }>('/dashboard-config/widgets', data),
  updateWidget: (id: number, data: UpdateWidget) => apiClient.put(`/dashboard-config/widgets/${id}`, data),
  deleteWidget: (id: number) => apiClient.delete(`/dashboard-config/widgets/${id}`),
  reorderWidgets: (dashboardId: number, orders: WidgetOrder[]) => 
    apiClient.post(`/dashboard-config/dashboards/${dashboardId}/reorder-widgets`, orders),

  // Meta
  getWidgetTypes: () => apiClient.get<WidgetTypeOption[]>('/dashboard-config/widget-types'),
  getDataSources: () => apiClient.get<DataSource[]>('/dashboard-config/data-sources'),
  initializeDefaults: () => apiClient.post<{ message: string; count: number }>('/dashboard-config/initialize', {}),
};

// Dashboard Data Service (for fetching actual widget data)
export const dashboardDataService = {
  getStats: () => apiClient.get<DashboardStats>('/dashboard/stats'),
  getPipeline: () => apiClient.get<PipelineSummary>('/dashboard/pipeline'),
  getActivities: (count?: number) => apiClient.get<RecentActivity[]>(`/dashboard/activities${count ? `?count=${count}` : ''}`),
};

// Helper functions
export const getWidgetTypeIcon = (type: WidgetType): string => {
  const icons: Record<WidgetType, string> = {
    [WidgetType.StatCard]: 'Assessment',
    [WidgetType.LineChart]: 'ShowChart',
    [WidgetType.BarChart]: 'BarChart',
    [WidgetType.PieChart]: 'PieChart',
    [WidgetType.DataTable]: 'TableChart',
    [WidgetType.ActivityList]: 'History',
    [WidgetType.TaskList]: 'Assignment',
    [WidgetType.PipelineFunnel]: 'FilterAlt',
    [WidgetType.ProgressGauge]: 'Speed',
    [WidgetType.MapWidget]: 'Map',
    [WidgetType.CalendarWidget]: 'CalendarMonth',
    [WidgetType.CustomContent]: 'Article',
    [WidgetType.Leaderboard]: 'Leaderboard',
    [WidgetType.KPICard]: 'TrendingUp',
    [WidgetType.AreaChart]: 'AreaChart',
    [WidgetType.StackedBarChart]: 'StackedBarChart'
  };
  return icons[type] || 'Widgets';
};

export const formatWidgetValue = (value: number | undefined, type: string): string => {
  if (value === undefined || value === null) return '--';
  
  switch (type) {
    case 'currency':
      if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
      if (value >= 1000) return `$${(value / 1000).toFixed(1)}K`;
      return `$${value.toLocaleString()}`;
    case 'percentage':
      return `${value.toFixed(1)}%`;
    case 'count':
    default:
      return value.toLocaleString();
  }
};

export default dashboardConfigService;
