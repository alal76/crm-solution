/**
 * Account Management Components
 * Exports all account-related components for easy importing
 */

// PRA-020: AccountMergeDialog — component file does not exist yet.
// Merge API IS available: POST /api/duplicates/merge (DuplicatesController).
// TODO: Create AccountMergeDialog.tsx and re-enable export once component is built.
// export { AccountMergeDialog } from './AccountMergeDialog';
// export type { AccountMergeDialogProps } from './AccountMergeDialog';

// PRA-020: AccountHierarchyTree — component file does not exist yet.
// @mui/x-tree-view v8.27.1 IS installed (package.json).
// TODO: Create AccountHierarchyTree.tsx using SimpleTreeView/RichTreeView from @mui/x-tree-view.
// export { AccountHierarchyTree } from './AccountHierarchyTree';
// export type { AccountHierarchyTreeProps } from './AccountHierarchyTree';

export { TerritoryAssignmentPanel } from './TerritoryAssignmentPanel';
export type { TerritoryAssignmentPanelProps } from './TerritoryAssignmentPanel';

// PRA-020: AccountTimeline — component file does not exist yet.
// No activityService exists in src/services/; InteractionsController at /api/interactions is available.
// TODO: Create AccountTimeline.tsx using /api/interactions?accountId={id} for activity data.
// export { AccountTimeline } from './AccountTimeline';
// export type { AccountTimelineProps } from './AccountTimeline';
