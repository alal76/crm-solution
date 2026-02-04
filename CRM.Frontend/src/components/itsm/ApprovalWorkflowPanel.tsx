// Approval Workflow Panel - Multi-level approval UI for changes
// Part of ITSM Enhancement Plan - Phase 2.2

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Button,
  TextField,
  Avatar,
  Stack,
  Chip,
  Divider,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Tooltip,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
} from '@mui/material';
import {
  CheckCircle as ApprovedIcon,
  Cancel as RejectedIcon,
  HourglassEmpty as PendingIcon,
  Person as PersonIcon,
  Comment as CommentIcon,
  Close as CloseIcon,
  ThumbUp as ApproveIcon,
  ThumbDown as RejectIcon,
  Schedule as ScheduleIcon,
} from '@mui/icons-material';

export type ApprovalStatus = 'pending' | 'approved' | 'rejected' | 'skipped';

export interface Approver {
  id: number;
  name: string;
  email: string;
  role: string;
  avatarUrl?: string;
}

export interface ApprovalStep {
  id: number;
  name: string;
  description?: string;
  approvers: Approver[];
  status: ApprovalStatus;
  approvedBy?: Approver;
  approvedAt?: Date | string;
  comments?: string;
  required: boolean;
  order: number;
}

export interface ApprovalWorkflowPanelProps {
  steps: ApprovalStep[];
  currentUserId: number;
  onApprove?: (stepId: number, comments: string) => Promise<void>;
  onReject?: (stepId: number, comments: string) => Promise<void>;
  onRequestApproval?: (stepId: number) => Promise<void>;
  readOnly?: boolean;
  title?: string;
}

const getStatusIcon = (status: ApprovalStatus) => {
  switch (status) {
    case 'approved':
      return <ApprovedIcon sx={{ color: '#4caf50' }} />;
    case 'rejected':
      return <RejectedIcon sx={{ color: '#f44336' }} />;
    case 'pending':
      return <PendingIcon sx={{ color: '#ff9800' }} />;
    case 'skipped':
      return <ScheduleIcon sx={{ color: '#9e9e9e' }} />;
    default:
      return <PendingIcon />;
  }
};

const getStatusColor = (status: ApprovalStatus): string => {
  switch (status) {
    case 'approved':
      return '#4caf50';
    case 'rejected':
      return '#f44336';
    case 'pending':
      return '#ff9800';
    case 'skipped':
      return '#9e9e9e';
    default:
      return '#9e9e9e';
  }
};

interface ApprovalDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (comments: string) => void;
  action: 'approve' | 'reject';
  stepName: string;
  loading?: boolean;
}

const ApprovalDialog: React.FC<ApprovalDialogProps> = ({
  open,
  onClose,
  onConfirm,
  action,
  stepName,
  loading = false,
}) => {
  const [comments, setComments] = useState('');

  const handleConfirm = () => {
    onConfirm(comments);
    setComments('');
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Stack direction="row" alignItems="center" justifyContent="space-between">
          <Typography variant="h6">
            {action === 'approve' ? 'Approve' : 'Reject'} - {stepName}
          </Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Stack>
      </DialogTitle>
      <DialogContent>
        <TextField
          fullWidth
          multiline
          rows={4}
          label={action === 'reject' ? 'Reason for Rejection (Required)' : 'Comments (Optional)'}
          value={comments}
          onChange={(e) => setComments(e.target.value)}
          required={action === 'reject'}
          placeholder={
            action === 'approve'
              ? 'Add any comments or conditions for approval...'
              : 'Please provide a reason for rejection...'
          }
          sx={{ mt: 2 }}
        />
        {action === 'reject' && (
          <Alert severity="warning" sx={{ mt: 2 }}>
            Rejecting this approval will halt the change process. The requestor will be notified.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          color={action === 'approve' ? 'success' : 'error'}
          onClick={handleConfirm}
          disabled={loading || (action === 'reject' && !comments.trim())}
          startIcon={action === 'approve' ? <ApproveIcon /> : <RejectIcon />}
        >
          {action === 'approve' ? 'Approve' : 'Reject'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export const ApprovalWorkflowPanel: React.FC<ApprovalWorkflowPanelProps> = ({
  steps,
  currentUserId,
  onApprove,
  onReject,
  onRequestApproval,
  readOnly = false,
  title = 'Approval Workflow',
}) => {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogAction, setDialogAction] = useState<'approve' | 'reject'>('approve');
  const [selectedStep, setSelectedStep] = useState<ApprovalStep | null>(null);
  const [loading, setLoading] = useState(false);

  // Sort steps by order
  const sortedSteps = [...steps].sort((a, b) => a.order - b.order);

  // Find active step (first pending step)
  const activeStepIndex = sortedSteps.findIndex((s) => s.status === 'pending');

  // Check if current user can approve the active step
  const canApprove = (step: ApprovalStep): boolean => {
    if (readOnly) return false;
    if (step.status !== 'pending') return false;
    return step.approvers.some((a) => a.id === currentUserId);
  };

  const handleOpenDialog = (step: ApprovalStep, action: 'approve' | 'reject') => {
    setSelectedStep(step);
    setDialogAction(action);
    setDialogOpen(true);
  };

  const handleConfirm = async (comments: string) => {
    if (!selectedStep) return;
    setLoading(true);
    try {
      if (dialogAction === 'approve') {
        await onApprove?.(selectedStep.id, comments);
      } else {
        await onReject?.(selectedStep.id, comments);
      }
      setDialogOpen(false);
    } catch (error) {
      console.error('Approval action failed:', error);
    } finally {
      setLoading(false);
    }
  };

  // Calculate overall status
  const overallStatus = (): { status: string; color: string } => {
    if (sortedSteps.some((s) => s.status === 'rejected')) {
      return { status: 'Rejected', color: '#f44336' };
    }
    if (sortedSteps.every((s) => s.status === 'approved' || s.status === 'skipped')) {
      return { status: 'Fully Approved', color: '#4caf50' };
    }
    const approvedCount = sortedSteps.filter((s) => s.status === 'approved').length;
    return {
      status: `${approvedCount}/${sortedSteps.length} Approved`,
      color: '#ff9800',
    };
  };

  const overall = overallStatus();

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Typography variant="subtitle1" fontWeight={600}>
          {title}
        </Typography>
        <Chip
          label={overall.status}
          size="small"
          sx={{
            backgroundColor: `${overall.color}20`,
            color: overall.color,
            fontWeight: 600,
          }}
        />
      </Stack>

      <Stepper activeStep={activeStepIndex} orientation="vertical">
        {sortedSteps.map((step, index) => (
          <Step key={step.id} completed={step.status === 'approved'}>
            <StepLabel
              StepIconComponent={() => getStatusIcon(step.status)}
              optional={
                step.status === 'approved' && step.approvedAt ? (
                  <Typography variant="caption" color="text.secondary">
                    {new Date(step.approvedAt).toLocaleDateString()} by {step.approvedBy?.name}
                  </Typography>
                ) : null
              }
            >
              <Stack direction="row" alignItems="center" spacing={1}>
                <Typography fontWeight={step.status === 'pending' ? 600 : 400}>
                  {step.name}
                </Typography>
                {step.required && (
                  <Chip label="Required" size="small" variant="outlined" />
                )}
              </Stack>
            </StepLabel>
            <StepContent>
              {step.description && (
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  {step.description}
                </Typography>
              )}

              {/* Approvers list */}
              <Typography variant="caption" color="text.secondary">
                Approvers:
              </Typography>
              <List dense sx={{ py: 0 }}>
                {step.approvers.map((approver) => (
                  <ListItem key={approver.id} sx={{ px: 0 }}>
                    <ListItemAvatar>
                      <Avatar
                        src={approver.avatarUrl}
                        sx={{ width: 32, height: 32 }}
                      >
                        {approver.name.charAt(0)}
                      </Avatar>
                    </ListItemAvatar>
                    <ListItemText
                      primary={approver.name}
                      secondary={approver.role}
                      primaryTypographyProps={{ variant: 'body2' }}
                      secondaryTypographyProps={{ variant: 'caption' }}
                    />
                    {approver.id === currentUserId && step.status === 'pending' && (
                      <Chip
                        label="Your approval needed"
                        size="small"
                        color="warning"
                        variant="outlined"
                      />
                    )}
                  </ListItem>
                ))}
              </List>

              {/* Comments if any */}
              {step.comments && (
                <Box
                  sx={{
                    mt: 1,
                    p: 1,
                    borderRadius: 1,
                    backgroundColor: 'action.hover',
                  }}
                >
                  <Stack direction="row" spacing={1} alignItems="flex-start">
                    <CommentIcon fontSize="small" color="action" />
                    <Typography variant="body2">{step.comments}</Typography>
                  </Stack>
                </Box>
              )}

              {/* Action buttons */}
              {canApprove(step) && (
                <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
                  <Button
                    variant="contained"
                    color="success"
                    size="small"
                    startIcon={<ApproveIcon />}
                    onClick={() => handleOpenDialog(step, 'approve')}
                  >
                    Approve
                  </Button>
                  <Button
                    variant="outlined"
                    color="error"
                    size="small"
                    startIcon={<RejectIcon />}
                    onClick={() => handleOpenDialog(step, 'reject')}
                  >
                    Reject
                  </Button>
                </Stack>
              )}

              {/* Status indicator for completed steps */}
              {step.status === 'rejected' && (
                <Alert severity="error" sx={{ mt: 1 }}>
                  Rejected by {step.approvedBy?.name}
                  {step.comments && `: ${step.comments}`}
                </Alert>
              )}
            </StepContent>
          </Step>
        ))}
      </Stepper>

      {/* Approval Dialog */}
      <ApprovalDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onConfirm={handleConfirm}
        action={dialogAction}
        stepName={selectedStep?.name || ''}
        loading={loading}
      />
    </Paper>
  );
};

export default ApprovalWorkflowPanel;
