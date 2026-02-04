// Incident Timeline - Activity timeline for incidents
// Part of ITSM Enhancement Plan - Phase 1.1

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Stack,
  Avatar,
  Chip,
  IconButton,
  Tooltip,
  Collapse,
  Divider,
  Button,
} from '@mui/material';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineOppositeContent,
} from '@mui/lab';
import {
  Add as CreateIcon,
  PersonAdd as AssignIcon,
  Edit as UpdateIcon,
  Comment as CommentIcon,
  AttachFile as AttachIcon,
  ArrowUpward as EscalateIcon,
  CheckCircle as ResolveIcon,
  Lock as CloseIcon,
  Refresh as ReopenIcon,
  Pause as HoldIcon,
  PlayArrow as ResumeIcon,
  LinkOff as UnlinkIcon,
  Link as LinkIcon,
  Article as ArticleIcon,
  Warning as SLAIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  FilterList as FilterIcon,
} from '@mui/icons-material';

export type ActivityType =
  | 'created'
  | 'assigned'
  | 'updated'
  | 'commented'
  | 'attachment_added'
  | 'escalated'
  | 'resolved'
  | 'closed'
  | 'reopened'
  | 'on_hold'
  | 'resumed'
  | 'linked_problem'
  | 'unlinked_problem'
  | 'article_attached'
  | 'sla_breach';

export interface TimelineActivity {
  id: number;
  type: ActivityType;
  timestamp: Date | string;
  userId: number;
  userName: string;
  userAvatar?: string;
  details: string;
  metadata?: Record<string, any>;
}

export interface IncidentTimelineProps {
  activities: TimelineActivity[];
  showFilters?: boolean;
  maxInitialDisplay?: number;
  onLoadMore?: () => Promise<void>;
  hasMore?: boolean;
}

const getActivityIcon = (type: ActivityType) => {
  switch (type) {
    case 'created':
      return <CreateIcon />;
    case 'assigned':
      return <AssignIcon />;
    case 'updated':
      return <UpdateIcon />;
    case 'commented':
      return <CommentIcon />;
    case 'attachment_added':
      return <AttachIcon />;
    case 'escalated':
      return <EscalateIcon />;
    case 'resolved':
      return <ResolveIcon />;
    case 'closed':
      return <CloseIcon />;
    case 'reopened':
      return <ReopenIcon />;
    case 'on_hold':
      return <HoldIcon />;
    case 'resumed':
      return <ResumeIcon />;
    case 'linked_problem':
      return <LinkIcon />;
    case 'unlinked_problem':
      return <UnlinkIcon />;
    case 'article_attached':
      return <ArticleIcon />;
    case 'sla_breach':
      return <SLAIcon />;
    default:
      return <UpdateIcon />;
  }
};

const getActivityColor = (type: ActivityType): 'success' | 'error' | 'warning' | 'info' | 'grey' => {
  switch (type) {
    case 'created':
      return 'info';
    case 'assigned':
      return 'info';
    case 'resolved':
      return 'success';
    case 'closed':
      return 'success';
    case 'escalated':
      return 'warning';
    case 'sla_breach':
      return 'error';
    case 'reopened':
      return 'warning';
    case 'on_hold':
      return 'grey';
    default:
      return 'grey';
  }
};

const getActivityLabel = (type: ActivityType): string => {
  const labels: Record<ActivityType, string> = {
    created: 'Created',
    assigned: 'Assigned',
    updated: 'Updated',
    commented: 'Comment',
    attachment_added: 'Attachment',
    escalated: 'Escalated',
    resolved: 'Resolved',
    closed: 'Closed',
    reopened: 'Reopened',
    on_hold: 'On Hold',
    resumed: 'Resumed',
    linked_problem: 'Linked to Problem',
    unlinked_problem: 'Unlinked from Problem',
    article_attached: 'Article Attached',
    sla_breach: 'SLA Breach',
  };
  return labels[type] || type;
};

const formatTimestamp = (timestamp: Date | string): { date: string; time: string; relative: string } => {
  const date = new Date(timestamp);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / (1000 * 60));
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  let relative: string;
  if (diffMins < 1) {
    relative = 'Just now';
  } else if (diffMins < 60) {
    relative = `${diffMins}m ago`;
  } else if (diffHours < 24) {
    relative = `${diffHours}h ago`;
  } else if (diffDays < 7) {
    relative = `${diffDays}d ago`;
  } else {
    relative = date.toLocaleDateString();
  }

  return {
    date: date.toLocaleDateString(),
    time: date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    relative,
  };
};

interface ActivityItemProps {
  activity: TimelineActivity;
  isLast: boolean;
}

const ActivityItem: React.FC<ActivityItemProps> = ({ activity, isLast }) => {
  const [expanded, setExpanded] = useState(false);
  const timestamp = formatTimestamp(activity.timestamp);
  const color = getActivityColor(activity.type);
  const hasMetadata = activity.metadata && Object.keys(activity.metadata).length > 0;

  return (
    <TimelineItem>
      <TimelineOppositeContent sx={{ flex: 0.2, minWidth: 100 }}>
        <Tooltip title={`${timestamp.date} ${timestamp.time}`}>
          <Typography variant="caption" color="text.secondary">
            {timestamp.relative}
          </Typography>
        </Tooltip>
      </TimelineOppositeContent>

      <TimelineSeparator>
        <TimelineDot color={color} variant={activity.type === 'commented' ? 'outlined' : 'filled'}>
          {getActivityIcon(activity.type)}
        </TimelineDot>
        {!isLast && <TimelineConnector />}
      </TimelineSeparator>

      <TimelineContent sx={{ pb: 2 }}>
        <Paper
          variant="outlined"
          sx={{
            p: 1.5,
            backgroundColor: activity.type === 'sla_breach' ? '#fff3e0' : undefined,
          }}
        >
          <Stack direction="row" alignItems="flex-start" spacing={1}>
            <Avatar
              src={activity.userAvatar}
              sx={{ width: 28, height: 28, fontSize: '0.8rem' }}
            >
              {activity.userName.charAt(0)}
            </Avatar>

            <Box sx={{ flexGrow: 1 }}>
              <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap">
                <Typography variant="body2" fontWeight={600}>
                  {activity.userName}
                </Typography>
                <Chip
                  label={getActivityLabel(activity.type)}
                  size="small"
                  color={color === 'grey' ? 'default' : color}
                  variant={activity.type === 'commented' ? 'outlined' : 'filled'}
                  sx={{ height: 20, fontSize: '0.65rem' }}
                />
              </Stack>

              <Typography variant="body2" sx={{ mt: 0.5 }}>
                {activity.details}
              </Typography>

              {/* Expandable metadata */}
              {hasMetadata && (
                <>
                  <Button
                    size="small"
                    onClick={() => setExpanded(!expanded)}
                    endIcon={expanded ? <CollapseIcon /> : <ExpandIcon />}
                    sx={{ mt: 0.5, p: 0, minWidth: 'auto' }}
                  >
                    {expanded ? 'Hide details' : 'Show details'}
                  </Button>
                  <Collapse in={expanded}>
                    <Box
                      sx={{
                        mt: 1,
                        p: 1,
                        borderRadius: 1,
                        backgroundColor: 'action.hover',
                        fontFamily: 'monospace',
                        fontSize: '0.75rem',
                      }}
                    >
                      {Object.entries(activity.metadata!).map(([key, value]) => (
                        <Box key={key}>
                          <Typography
                            variant="caption"
                            component="span"
                            color="text.secondary"
                          >
                            {key}:
                          </Typography>{' '}
                          <Typography variant="caption" component="span">
                            {typeof value === 'object' ? JSON.stringify(value) : String(value)}
                          </Typography>
                        </Box>
                      ))}
                    </Box>
                  </Collapse>
                </>
              )}
            </Box>
          </Stack>
        </Paper>
      </TimelineContent>
    </TimelineItem>
  );
};

export const IncidentTimeline: React.FC<IncidentTimelineProps> = ({
  activities,
  showFilters = true,
  maxInitialDisplay = 10,
  onLoadMore,
  hasMore = false,
}) => {
  const [showAll, setShowAll] = useState(false);
  const [filterTypes, setFilterTypes] = useState<ActivityType[]>([]);
  const [showFilterMenu, setShowFilterMenu] = useState(false);

  // Filter activities
  const filteredActivities = filterTypes.length > 0
    ? activities.filter((a) => filterTypes.includes(a.type))
    : activities;

  // Limit display
  const displayedActivities = showAll
    ? filteredActivities
    : filteredActivities.slice(0, maxInitialDisplay);

  const toggleFilter = (type: ActivityType) => {
    setFilterTypes((prev) =>
      prev.includes(type) ? prev.filter((t) => t !== type) : [...prev, type]
    );
  };

  // Get unique activity types for filter
  const availableTypes = [...new Set(activities.map((a) => a.type))];

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Typography variant="subtitle1" fontWeight={600}>
          Activity Timeline
        </Typography>
        <Stack direction="row" spacing={1} alignItems="center">
          <Chip
            size="small"
            label={`${filteredActivities.length} activities`}
            variant="outlined"
          />
          {showFilters && (
            <Tooltip title="Filter activities">
              <IconButton size="small" onClick={() => setShowFilterMenu(!showFilterMenu)}>
                <FilterIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      </Stack>

      {/* Filters */}
      <Collapse in={showFilterMenu}>
        <Box sx={{ mb: 2, p: 1, backgroundColor: 'action.hover', borderRadius: 1 }}>
          <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
            Filter by type:
          </Typography>
          <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
            {availableTypes.map((type) => (
              <Chip
                key={type}
                size="small"
                label={getActivityLabel(type)}
                onClick={() => toggleFilter(type)}
                color={filterTypes.includes(type) ? 'primary' : 'default'}
                variant={filterTypes.includes(type) ? 'filled' : 'outlined'}
                sx={{ mb: 0.5 }}
              />
            ))}
          </Stack>
          {filterTypes.length > 0 && (
            <Button size="small" onClick={() => setFilterTypes([])} sx={{ mt: 1 }}>
              Clear filters
            </Button>
          )}
        </Box>
      </Collapse>

      {/* Timeline */}
      {displayedActivities.length === 0 ? (
        <Box sx={{ py: 3, textAlign: 'center' }}>
          <Typography color="text.secondary">No activities found</Typography>
        </Box>
      ) : (
        <Timeline sx={{ p: 0, m: 0 }}>
          {displayedActivities.map((activity, index) => (
            <ActivityItem
              key={activity.id}
              activity={activity}
              isLast={index === displayedActivities.length - 1}
            />
          ))}
        </Timeline>
      )}

      {/* Load more */}
      <Stack direction="row" justifyContent="center" spacing={1} sx={{ mt: 2 }}>
        {filteredActivities.length > maxInitialDisplay && (
          <Button size="small" onClick={() => setShowAll(!showAll)}>
            {showAll
              ? 'Show less'
              : `Show ${filteredActivities.length - maxInitialDisplay} more`}
          </Button>
        )}
        {hasMore && onLoadMore && (
          <Button size="small" variant="outlined" onClick={onLoadMore}>
            Load older activities
          </Button>
        )}
      </Stack>
    </Paper>
  );
};

export default IncidentTimeline;
