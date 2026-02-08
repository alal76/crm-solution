/**
 * Analytics Components Index
 * 
 * Exports all analytics-related components for the CRM Frontend.
 */

export { DashboardBuilder } from './DashboardBuilder';
export type { 
  WidgetType, 
  WidgetSize, 
  DataSource, 
  WidgetConfig, 
  DashboardConfig 
} from './DashboardBuilder';

export { ReportDesigner } from './ReportDesigner';
export type {
  ReportDataSource,
  ColumnType,
  FilterOperator,
  SortDirection,
  AggregationFunction,
  ReportColumn,
  ReportFilter,
  ReportSort,
  ReportGrouping,
  ReportSchedule,
  ReportConfig,
} from './ReportDesigner';
