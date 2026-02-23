/**
 * Marketing Components Barrel Export
 */

export { default as CampaignWizard } from './CampaignWizard';
export { default as EmailTemplateEditor } from './EmailTemplateEditor';
export { default as CampaignMetricsCard } from './CampaignMetricsCard';
export { default as AudienceSegmentBuilder } from './AudienceSegmentBuilder';
export { default as CampaignCalendar } from './CampaignCalendar';

// Re-export types
export type { CampaignWizardProps, CreateCampaignData } from './CampaignWizard';
export type { EmailTemplateEditorProps, EmailTemplateData } from './EmailTemplateEditor';
export type { CampaignMetricsCardProps, CampaignMetricsData } from './CampaignMetricsCard';
export type { AudienceSegmentBuilderProps, SegmentRule } from './AudienceSegmentBuilder';
export type { CampaignCalendarProps, CampaignEvent } from './CampaignCalendar';
