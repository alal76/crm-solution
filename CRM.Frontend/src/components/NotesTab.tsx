/**
 * NotesTab Component
 * 
 * A reusable tab component for displaying and managing notes attached to any entity.
 * Features:
 * - List all notes for the entity
 * - Create new notes
 * - Edit notes (if user has permission)
 * - Delete notes (if user has permission)
 * - Pin/unpin notes
 * - Mark as important
 * - Filter by note type
 * - RBAC: Only creator or admin roles can edit/delete
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  TextField,
  Card,
  CardContent,
  CardActions,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Alert,
  CircularProgress,
  Tooltip,
  Divider,
  Avatar,
  Menu,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PushPin as PinIcon,
  PushPinOutlined as UnpinIcon,
  Star as StarIcon,
  StarBorder as StarBorderIcon,
  MoreVert as MoreIcon,
  Note as NoteIcon,
  Phone as PhoneIcon,
  MeetingRoom as MeetingIcon,
  Feedback as FeedbackIcon,
  Warning as WarningIcon,
  Lightbulb as IdeaIcon,
  BugReport as IssueIcon,
  FormatListBulleted as RequirementIcon,
  MoreHoriz as OtherIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { EntityType } from '../contexts/EntityContext';

// Note types matching backend enum
export enum NoteType {
  General = 0,
  CallNotes = 1,
  MeetingNotes = 2,
  Feedback = 3,
  Requirement = 4,
  Issue = 5,
  Idea = 6,
  Warning = 7,
  Other = 8,
}

export enum NoteVisibility {
  Private = 0,
  Team = 1,
  Public = 2,
}

const noteTypeLabels: Record<NoteType, string> = {
  [NoteType.General]: 'General',
  [NoteType.CallNotes]: 'Call Notes',
  [NoteType.MeetingNotes]: 'Meeting Notes',
  [NoteType.Feedback]: 'Feedback',
  [NoteType.Requirement]: 'Requirement',
  [NoteType.Issue]: 'Issue',
  [NoteType.Idea]: 'Idea',
  [NoteType.Warning]: 'Warning',
  [NoteType.Other]: 'Other',
};

const noteTypeIcons: Record<NoteType, React.ReactNode> = {
  [NoteType.General]: <NoteIcon fontSize="small" />,
  [NoteType.CallNotes]: <PhoneIcon fontSize="small" />,
  [NoteType.MeetingNotes]: <MeetingIcon fontSize="small" />,
  [NoteType.Feedback]: <FeedbackIcon fontSize="small" />,
  [NoteType.Requirement]: <RequirementIcon fontSize="small" />,
  [NoteType.Issue]: <IssueIcon fontSize="small" />,
  [NoteType.Idea]: <IdeaIcon fontSize="small" />,
  [NoteType.Warning]: <WarningIcon fontSize="small" />,
  [NoteType.Other]: <OtherIcon fontSize="small" />,
};

const noteTypeColors: Record<NoteType, 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning'> = {
  [NoteType.General]: 'default',
  [NoteType.CallNotes]: 'primary',
  [NoteType.MeetingNotes]: 'info',
  [NoteType.Feedback]: 'secondary',
  [NoteType.Requirement]: 'success',
  [NoteType.Issue]: 'error',
  [NoteType.Idea]: 'warning',
  [NoteType.Warning]: 'warning',
  [NoteType.Other]: 'default',
};

const visibilityLabels: Record<NoteVisibility, string> = {
  [NoteVisibility.Private]: 'Private',
  [NoteVisibility.Team]: 'Team',
  [NoteVisibility.Public]: 'Public',
};

interface NoteResponse {
  id: number;
  title: string;
  content: string;
  summary?: string;
  noteType: NoteType;
  visibility: NoteVisibility;
  isPinned: boolean;
  isImportant: boolean;
  entityType?: string;
  entityId?: number;
  createdByUserId?: number;
  createdByUserName?: string;
  lastModifiedByUserId?: number;
  lastModifiedByUserName?: string;
  tags?: string;
  category?: string;
  attachments?: string;
  contextPath?: string;
  createdAt: string;
  updatedAt?: string;
  canEdit: boolean;
  canDelete: boolean;
}

interface NoteFormData {
  title: string;
  content: string;
  noteType: NoteType;
  visibility: NoteVisibility;
  isPinned: boolean;
  isImportant: boolean;
  tags?: string;
}

interface NotesTabProps {
  entityType: EntityType;
  entityId: number;
  entityName?: string;
  readOnly?: boolean;
  maxHeight?: number | string;
}

const NotesTab: React.FC<NotesTabProps> = ({
  entityType,
  entityId,
  entityName,
  readOnly = false,
  maxHeight = 400,
}) => {
  const [notes, setNotes] = useState<NoteResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<NoteType | -1>(-1);
  
  // Dialog states
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingNote, setEditingNote] = useState<NoteResponse | null>(null);
  const [formData, setFormData] = useState<NoteFormData>({
    title: '',
    content: '',
    noteType: NoteType.General,
    visibility: NoteVisibility.Team,
    isPinned: false,
    isImportant: false,
    tags: '',
  });
  const [saving, setSaving] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [noteToDelete, setNoteToDelete] = useState<NoteResponse | null>(null);
  
  // Menu state
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
  const [menuNote, setMenuNote] = useState<NoteResponse | null>(null);

  const fetchNotes = useCallback(async () => {
    if (!entityType || !entityId) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiClient.get<NoteResponse[]>(
        `/notes/entity/${entityType}/${entityId}`
      );
      setNotes(response.data || []);
    } catch (err: any) {
      console.error('Failed to load notes:', err);
      setError(err.response?.data?.message || 'Failed to load notes');
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    fetchNotes();
  }, [fetchNotes]);

  const handleOpenDialog = (note?: NoteResponse) => {
    if (note) {
      setEditingNote(note);
      setFormData({
        title: note.title,
        content: note.content,
        noteType: note.noteType,
        visibility: note.visibility,
        isPinned: note.isPinned,
        isImportant: note.isImportant,
        tags: note.tags || '',
      });
    } else {
      setEditingNote(null);
      setFormData({
        title: '',
        content: '',
        noteType: NoteType.General,
        visibility: NoteVisibility.Team,
        isPinned: false,
        isImportant: false,
        tags: '',
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingNote(null);
  };

  const handleSave = async () => {
    if (!formData.content.trim()) return;
    
    setSaving(true);
    try {
      const payload = {
        ...formData,
        id: editingNote?.id ?? 0,
        entityType,
        entityId,
        title: formData.title || `${noteTypeLabels[formData.noteType]} - ${new Date().toLocaleDateString()}`,
      };

      if (editingNote) {
        await apiClient.put(`/notes/${editingNote.id}`, payload);
      } else {
        await apiClient.post('/notes', payload);
      }
      
      await fetchNotes();
      handleCloseDialog();
    } catch (err: any) {
      console.error('Failed to save note:', err);
      setError(err.response?.data?.message || 'Failed to save note');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!noteToDelete) return;
    
    try {
      await apiClient.delete(`/notes/${noteToDelete.id}`);
      await fetchNotes();
      setDeleteConfirmOpen(false);
      setNoteToDelete(null);
    } catch (err: any) {
      console.error('Failed to delete note:', err);
      setError(err.response?.data?.message || 'Failed to delete note');
    }
  };

  const handleTogglePin = async (note: NoteResponse) => {
    try {
      await apiClient.post(`/notes/${note.id}/toggle-pin`);
      await fetchNotes();
    } catch (err) {
      console.error('Failed to toggle pin:', err);
    }
  };

  const handleToggleImportant = async (note: NoteResponse) => {
    try {
      await apiClient.post(`/notes/${note.id}/toggle-important`);
      await fetchNotes();
    } catch (err) {
      console.error('Failed to toggle important:', err);
    }
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, note: NoteResponse) => {
    setMenuAnchor(event.currentTarget);
    setMenuNote(note);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setMenuNote(null);
  };

  const filteredNotes = filterType === -1 
    ? notes 
    : notes.filter(n => n.noteType === filterType);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" py={4}>
        <CircularProgress size={32} />
      </Box>
    );
  }

  return (
    <Box>
      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Header with Add button and filter */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography variant="subtitle2" color="text.secondary">
            {filteredNotes.length} note{filteredNotes.length !== 1 ? 's' : ''}
            {entityName && ` for ${entityName}`}
          </Typography>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <Select
              value={filterType}
              onChange={(e) => setFilterType(e.target.value as NoteType | -1)}
              displayEmpty
              size="small"
            >
              <MenuItem value={-1}>All Types</MenuItem>
              {Object.entries(noteTypeLabels).map(([value, label]) => (
                <MenuItem key={value} value={parseInt(value)}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    {noteTypeIcons[parseInt(value) as NoteType]}
                    <span>{label}</span>
                  </Stack>
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>
        
        {!readOnly && (
          <Button
            variant="contained"
            size="small"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
          >
            Add Note
          </Button>
        )}
      </Stack>

      {/* Notes List */}
      <Box sx={{ maxHeight, overflowY: 'auto' }}>
        {filteredNotes.length === 0 ? (
          <Box textAlign="center" py={4}>
            <NoteIcon sx={{ fontSize: 48, color: 'text.disabled' }} />
            <Typography color="text.secondary" mt={1}>
              No notes yet
            </Typography>
            {!readOnly && (
              <Button
                variant="text"
                size="small"
                startIcon={<AddIcon />}
                onClick={() => handleOpenDialog()}
                sx={{ mt: 1 }}
              >
                Add the first note
              </Button>
            )}
          </Box>
        ) : (
          <Stack spacing={1.5}>
            {filteredNotes.map((note) => (
              <Card 
                key={note.id} 
                variant="outlined"
                sx={{ 
                  borderLeft: note.isImportant ? 4 : 1,
                  borderLeftColor: note.isImportant ? 'warning.main' : 'divider',
                  bgcolor: note.isPinned ? 'action.hover' : 'background.paper',
                }}
              >
                <CardContent sx={{ pb: 1 }}>
                  <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                    <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap">
                      <Chip
                        size="small"
                        icon={noteTypeIcons[note.noteType] as React.ReactElement}
                        label={noteTypeLabels[note.noteType]}
                        color={noteTypeColors[note.noteType]}
                        variant="outlined"
                      />
                      {note.isPinned && (
                        <Chip size="small" icon={<PinIcon />} label="Pinned" color="primary" variant="filled" />
                      )}
                      {note.isImportant && (
                        <Chip size="small" icon={<StarIcon />} label="Important" color="warning" variant="filled" />
                      )}
                      <Chip size="small" label={visibilityLabels[note.visibility]} variant="outlined" />
                    </Stack>
                    
                    <IconButton size="small" onClick={(e) => handleMenuOpen(e, note)}>
                      <MoreIcon fontSize="small" />
                    </IconButton>
                  </Stack>
                  
                  {note.title && (
                    <Typography variant="subtitle2" fontWeight="bold" mt={1}>
                      {note.title}
                    </Typography>
                  )}
                  
                  <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mt: 0.5 }}>
                    {note.content}
                  </Typography>
                  
                  {note.tags && (
                    <Stack direction="row" spacing={0.5} mt={1} flexWrap="wrap">
                      {note.tags.split(',').map((tag, idx) => (
                        <Chip key={idx} label={tag.trim()} size="small" variant="outlined" />
                      ))}
                    </Stack>
                  )}
                </CardContent>
                
                <Divider />
                
                <CardActions sx={{ px: 2, py: 0.5, justifyContent: 'space-between' }}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <Avatar sx={{ width: 20, height: 20, fontSize: 10 }}>
                      {note.createdByUserName?.charAt(0) || '?'}
                    </Avatar>
                    <Typography variant="caption" color="text.secondary">
                      {note.createdByUserName || 'Unknown'} • {formatDate(note.createdAt)}
                      {note.updatedAt && note.updatedAt !== note.createdAt && ' (edited)'}
                    </Typography>
                  </Stack>
                  
                  <Stack direction="row" spacing={0.5}>
                    <Tooltip title={note.isPinned ? 'Unpin' : 'Pin'}>
                      <IconButton size="small" onClick={() => handleTogglePin(note)}>
                        {note.isPinned ? <PinIcon fontSize="small" color="primary" /> : <UnpinIcon fontSize="small" />}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={note.isImportant ? 'Unmark important' : 'Mark important'}>
                      <IconButton size="small" onClick={() => handleToggleImportant(note)}>
                        {note.isImportant ? <StarIcon fontSize="small" color="warning" /> : <StarBorderIcon fontSize="small" />}
                      </IconButton>
                    </Tooltip>
                  </Stack>
                </CardActions>
              </Card>
            ))}
          </Stack>
        )}
      </Box>

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        {menuNote?.canEdit && (
          <MenuItem onClick={() => { handleMenuClose(); handleOpenDialog(menuNote); }}>
            <ListItemIcon><EditIcon fontSize="small" /></ListItemIcon>
            <ListItemText>Edit</ListItemText>
          </MenuItem>
        )}
        <MenuItem onClick={() => { handleMenuClose(); if (menuNote) handleTogglePin(menuNote); }}>
          <ListItemIcon>{menuNote?.isPinned ? <UnpinIcon fontSize="small" /> : <PinIcon fontSize="small" />}</ListItemIcon>
          <ListItemText>{menuNote?.isPinned ? 'Unpin' : 'Pin'}</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => { handleMenuClose(); if (menuNote) handleToggleImportant(menuNote); }}>
          <ListItemIcon>{menuNote?.isImportant ? <StarBorderIcon fontSize="small" /> : <StarIcon fontSize="small" />}</ListItemIcon>
          <ListItemText>{menuNote?.isImportant ? 'Unmark Important' : 'Mark Important'}</ListItemText>
        </MenuItem>
        {menuNote?.canDelete && (
          <>
            <Divider />
            <MenuItem 
              onClick={() => { 
                handleMenuClose(); 
                setNoteToDelete(menuNote); 
                setDeleteConfirmOpen(true); 
              }}
              sx={{ color: 'error.main' }}
            >
              <ListItemIcon><DeleteIcon fontSize="small" color="error" /></ListItemIcon>
              <ListItemText>Delete</ListItemText>
            </MenuItem>
          </>
        )}
      </Menu>

      {/* Add/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingNote ? 'Edit Note' : 'Add Note'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Title (optional)"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              fullWidth
              size="small"
            />
            
            <TextField
              label="Content"
              value={formData.content}
              onChange={(e) => setFormData({ ...formData, content: e.target.value })}
              fullWidth
              multiline
              rows={4}
              required
            />
            
            <Stack direction="row" spacing={2}>
              <FormControl fullWidth size="small">
                <InputLabel>Type</InputLabel>
                <Select
                  value={formData.noteType}
                  label="Type"
                  onChange={(e) => setFormData({ ...formData, noteType: e.target.value as NoteType })}
                >
                  {Object.entries(noteTypeLabels).map(([value, label]) => (
                    <MenuItem key={value} value={parseInt(value)}>
                      <Stack direction="row" spacing={1} alignItems="center">
                        {noteTypeIcons[parseInt(value) as NoteType]}
                        <span>{label}</span>
                      </Stack>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              
              <FormControl fullWidth size="small">
                <InputLabel>Visibility</InputLabel>
                <Select
                  value={formData.visibility}
                  label="Visibility"
                  onChange={(e) => setFormData({ ...formData, visibility: e.target.value as NoteVisibility })}
                >
                  {Object.entries(visibilityLabels).map(([value, label]) => (
                    <MenuItem key={value} value={parseInt(value)}>{label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Stack>
            
            <TextField
              label="Tags (comma-separated)"
              value={formData.tags}
              onChange={(e) => setFormData({ ...formData, tags: e.target.value })}
              fullWidth
              size="small"
              placeholder="e.g., important, follow-up, billing"
            />
            
            <Stack direction="row" spacing={2}>
              <Button
                variant={formData.isPinned ? 'contained' : 'outlined'}
                size="small"
                startIcon={formData.isPinned ? <PinIcon /> : <UnpinIcon />}
                onClick={() => setFormData({ ...formData, isPinned: !formData.isPinned })}
              >
                {formData.isPinned ? 'Pinned' : 'Pin Note'}
              </Button>
              <Button
                variant={formData.isImportant ? 'contained' : 'outlined'}
                color="warning"
                size="small"
                startIcon={formData.isImportant ? <StarIcon /> : <StarBorderIcon />}
                onClick={() => setFormData({ ...formData, isImportant: !formData.isImportant })}
              >
                {formData.isImportant ? 'Important' : 'Mark Important'}
              </Button>
            </Stack>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button 
            onClick={handleSave} 
            variant="contained" 
            disabled={!formData.content.trim() || saving}
          >
            {saving ? <CircularProgress size={20} /> : editingNote ? 'Save' : 'Add Note'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle>Delete Note</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this note? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Cancel</Button>
          <Button onClick={handleDelete} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default NotesTab;
