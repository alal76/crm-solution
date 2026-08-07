/**
 * Account Management Components
 * Exports all account-related components for easy importing
 */

// REV-FE-001: AccountMergeDialog was never built as a separate component — the merge
// wizard need it was meant to cover is fully implemented by the generic
// components/duplicates/MergeDialog.tsx (entityType="Account"), already wired into
// AccountsPage.tsx and AccountOverviewPage.tsx. Nothing in the codebase references
// `AccountMergeDialog` by name, so the dead placeholder export is removed rather than
// aliased — a re-export would imply an `AccountMergeDialogProps` contract that was
// never defined and doesn't match MergeDialog's actual props.

// REV-FE-001: AccountHierarchyTree implemented using SimpleTreeView from @mui/x-tree-view.
export { AccountHierarchyTree } from './AccountHierarchyTree';
export type { AccountHierarchyTreeProps } from './AccountHierarchyTree';

export { TerritoryAssignmentPanel } from './TerritoryAssignmentPanel';
export type { TerritoryAssignmentPanelProps } from './TerritoryAssignmentPanel';

// PRA-020: AccountTimeline — component file does not exist yet.
// No activityService exists in src/services/; InteractionsController at /api/interactions is available.
// TODO: Create AccountTimeline.tsx using /api/interactions?accountId={id} for activity data.
// export { AccountTimeline } from './AccountTimeline';
// export type { AccountTimelineProps } from './AccountTimeline';
