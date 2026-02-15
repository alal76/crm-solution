/**
 * IncidentBulkActionTools - Multi-select toolbar for bulk operations on incidents
 */

import React from 'react';
import {
  Box,
  Button,
  Stack,
  Chip,
  Checkbox,
  TableCell,
  TableRow,
  Typography,
  Menu,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  SelectChangeEvent,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  Edit as EditIcon,
  Assignment as AssignIcon,
  GetApp as BulkDownloadIcon,
  MoreVert as MoreIcon,
} from '@mui/icons-material';
import { IncidentStatus } from '../../services/incidentService';

interface BulkActionToolsProps {
  selectedCount: number;
  onBulkDelete: () => Promise<void>;
  onBulkStatusChange: (status: IncidentStatus) => Promise<void>;
  onBulkAssign: (userId: number) => Promise<void>;
  loading?: boolean;
  onSelectAll?: (selected: boolean) => void;
  selectAllChecked?: boolean;
}

export const IncidentBulkActionTools: React.FC<BulkActionToolsProps> = ({
  selectedCount,
  onBulkDelete,
  onBulkStatusChange,
  onBulkAssign,
  loading = false,
  onSelectAll,
  selectAllChecked = false,
}) => {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [statusDialogOpen, setStatusDialogOpen] = React.useState(false);
  const [selectedStatus, setSelectedStatus] = React.useState<IncidentStatus>(IncidentStatus.InProgress);
  const menuOpen = Boolean(anchorEl);

  const handleDeleteClick = async () => {
    if (window.confirm(`Are you sure you want to delete ${selectedCount} incidents?`)) {
      await onBulkDelete();
    }
  };

  const handleStatusChange = async () => {
    await onBulkStatusChange(selectedStatus);
    setStatusDialogOpen(false);
  };

  if (selectedCount === 0) {
    return null;
  }

  return (
    <>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          p: 2,
          mb: 2,
          bgcolor: 'action.hover',
          borderRadius: 1,
          gap: 2,
        }}
      >
        <Checkbox
          checked={selectAllChecked}
          onChange={(e) => onSelectAll?.(e.target.checked)}
          disabled={loading}
        />
        <Typography variant="subtitle2" sx={{ flex: 1 }}>
          {selectedCount} item{selectedCount !== 1 ? 's' : ''} selected
        </Typography>
        <Stack direction="row" spacing={1}>
          <Button
            size="small"
            startIcon={<EditIcon />}
            onClick={() => setStatusDialogOpen(true)}
            disabled={loading}
          >
            Change Status
          </Button>
          <Button
            size="small"
            startIcon={<AssignIcon />}
            disabled={loading}
          >
            Assign
          </Button>
          <Button
            size="small"
            startIcon={<DeleteIcon />}
            color="error"
            onClick={handleDeleteClick}
            disabled={loading}
          >
            Delete
          </Button>
        </Stack>
      </Box>

      {/* Status Change Dialog */}
      <Dialog open={statusDialogOpen} onClose={() => setStatusDialogOpen(false)}>
        <DialogTitle>Change Status</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <FormControl fullWidth>
            <InputLabel>Status</InputLabel>
            <Select
              value={selectedStatus}
              onChange={(e: SelectChangeEvent) => setSelectedStatus(Number(e.target.value))}
              label="Status"
            >
              <MenuItem value={IncidentStatus.New}>New</MenuItem>
              <MenuItem value={IncidentStatus.InProgress}>In Progress</MenuItem>
              <MenuItem value={IncidentStatus.OnHold}>On Hold</MenuItem>
              <MenuItem value={IncidentStatus.Resolved}>Resolved</MenuItem>
              <MenuItem value={IncidentStatus.Closed}>Closed</MenuItem>
              <MenuItem value={IncidentStatus.Cancelled}>Cancelled</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStatusDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleStatusChange}
            variant="contained"
            disabled={loading}
          >
            Apply
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default IncidentBulkActionTools;
