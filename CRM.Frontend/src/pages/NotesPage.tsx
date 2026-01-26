import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Container, Alert, CircularProgress,
  TextField, FormControl, InputLabel, Select, MenuItem, Chip, Grid, IconButton,
  Tooltip, Dialog, DialogTitle, DialogContent, DialogActions, Paper, Avatar,
  SelectChangeEvent, InputAdornment, Divider
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  PushPin as PinIcon, Note as NoteIcon, Search as SearchIcon,
  Visibility as PublicIcon, Lock as PrivateIcon, People as TeamIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { BaseEntity } from '../types';
import logo from '../assets/logo.png';
import LookupSelect from '../components/LookupSelect';
import ImportExportButtons from '../components/ImportExportButtons';

// Enums matching backend
const NOTE_TYPES = [
  { value: 0, label: 'General', color: '#9e9e9e' },
  { value: 1, label: 'Meeting', color: '#2196f3' },
  { value: 2, label: 'Call', color: '#ff9800' },
  { value: 3, label: 'Email', color: '#00bcd4' },
  { value: 4, label: 'Follow-Up', color: '#9c27b0' },
  { value: 5, label: 'Important', color: '#f44336' },
  { value: 6, label: 'Task', color: '#4caf50' },
  { value: 7, label: 'Idea', color: '#ffeb3b' },
  { value: 8, label: 'Issue', color: '#e91e63' },
  { value: 9, label: 'Feedback', color: '#607d8b' },
];

const NOTE_VISIBILITY = [
  { value: 0, label: 'Private', icon: <PrivateIcon /> },
  { value: 1, label: 'Team', icon: <TeamIcon /> },
  { value: 2, label: 'Public', icon: <PublicIcon /> },
];

const ENTITY_TYPES = ['Account', 'Contact', 'Opportunity', 'Task', 'Quote', 'Campaign', 'Product'];

interface Note extends BaseEntity {
  title: string;
  content: string;
  noteType: number;
  visibility: number;
  entityType?: string;
  entityId?: number;
  entityName?: string;
  isPinned: boolean;
  createdByUserId?: number;
  createdByUser?: { firstName: string; lastName: string };
  mentions?: string;
  tags?: string;
}

interface NoteForm {
  title: string;
  content: string;
  noteType: number;
  visibility: number;
  entityType: string;
  entityId: number | '';
  mentions: string;
  tags: string;
}

function NotesPage() {
  const [notes, setNotes] = useState<Note[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<number | 'all'>('all');

  const emptyForm: NoteForm = {
    title: '', content: '', noteType: 0, visibility: 0,
    entityType: '', entityId: '', mentions: '', tags: '',
  };
  const [formData, setFormData] = useState<NoteForm>(emptyForm);

  useEffect(() => { fetchNotes(); }, []);

  const fetchNotes = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/notes');
      setNotes(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch notes');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (note?: Note) => {
    if (note) {
      setEditingId(note.id);
      setFormData({
        title: note.title, content: note.content, noteType: note.noteType,
        visibility: note.visibility, entityType: note.entityType || '',
        entityId: note.entityId || '', mentions: note.mentions || '',
        tags: note.tags || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveNote = async () => {
    if (!formData.content.trim()) {
      setError('Please enter note content');
      return;
    }
    try {
      const payload = {
        ...formData,
        entityId: formData.entityId || null,
        entityType: formData.entityType || null,
      };
      if (editingId) {
        await apiClient.put(`/notes/${editingId}`, payload);
        setSuccessMessage('Note updated successfully');
      } else {
        await apiClient.post('/notes', payload);
        setSuccessMessage('Note created successfully');
      }
      handleCloseDialog();
      fetchNotes();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save note');
    }
  };

  const handleTogglePin = async (id: number) => {
    try {
      await apiClient.put(`/notes/${id}/toggle-pin`);
      fetchNotes();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to toggle pin');
    }
  };

  const handleDeleteNote = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this note?')) {
      try {
        await apiClient.delete(`/notes/${id}`);
        setSuccessMessage('Note deleted successfully');
        fetchNotes();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete note');
      }
    }
  };

  const getNoteType = (value: number) => NOTE_TYPES.find(t => t.value === value);
  const getVisibility = (value: number) => NOTE_VISIBILITY.find(v => v.value === value);

  const filteredNotes = notes
    .filter(note => {
      if (typeFilter !== 'all' && note.noteType !== typeFilter) return false;
      if (!searchQuery) return true;
      const query = searchQuery.toLowerCase();
      return (
        note.title?.toLowerCase().includes(query) ||
        note.content?.toLowerCase().includes(query) ||
        note.tags?.toLowerCase().includes(query)
      );
    })
    .sort((a, b) => {
      // Pinned notes first, then by date
      if (a.isPinned && !b.isPinned) return -1;
      if (!a.isPinned && b.isPinned) return 1;
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Notes</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              size="small"
              placeholder="Search notes..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              InputProps={{
                startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
              }}
              sx={{ width: 250 }}
            />
            <LookupSelect
              category="NoteType"
              name="typeFilter"
              value={typeFilter}
              onChange={(e:any) => setTypeFilter(e.target.value)}
              label="Type"
              fallback={[{ value: 'all', label: 'All Types' }, ...NOTE_TYPES.map(t => ({ value: t.value, label: t.label }))]}
            />
            <ImportExportButtons entityType="notes" entityLabel="Notes" onImportComplete={fetchNotes} />
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
              Add Note
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Notes Grid */}
        <Grid container spacing={2}>
          {filteredNotes.map((note) => {
            const type = getNoteType(note.noteType);
            const visibility = getVisibility(note.visibility);

            return (
              <Grid item xs={12} md={6} lg={4} key={note.id}>
                <Card 
                  sx={{ 
                    height: '100%', 
                    display: 'flex', 
                    flexDirection: 'column',
                    borderLeft: `4px solid ${type?.color}`,
                    backgroundColor: note.isPinned ? '#fffde7' : 'white',
                  }}
                >
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <NoteIcon sx={{ color: type?.color, fontSize: 20 }} />
                        <Chip label={type?.label || 'General'} size="small" sx={{ backgroundColor: type?.color, color: 'white' }} />
                      </Box>
                      <Box>
                        <Tooltip title={note.isPinned ? 'Unpin' : 'Pin'}>
                          <IconButton size="small" onClick={() => handleTogglePin(note.id)}>
                            <PinIcon sx={{ color: note.isPinned ? '#f44336' : '#9e9e9e', fontSize: 18 }} />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(note)}>
                            <EditIcon sx={{ color: '#6750A4', fontSize: 18 }} />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteNote(note.id)}>
                            <DeleteIcon sx={{ color: '#f44336', fontSize: 18 }} />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </Box>

                    {note.title && (
                      <Typography variant="h6" gutterBottom sx={{ fontSize: 16 }}>
                        {note.title}
                      </Typography>
                    )}

                    <Typography 
                      variant="body2" 
                      color="textSecondary" 
                      sx={{ 
                        whiteSpace: 'pre-wrap',
                        overflow: 'hidden',
                        display: '-webkit-box',
                        WebkitLineClamp: 5,
                        WebkitBoxOrient: 'vertical',
                        mb: 2,
                      }}
                    >
                      {note.content}
                    </Typography>

                    {note.entityType && note.entityName && (
                      <Chip 
                        label={`${note.entityType}: ${note.entityName}`} 
                        size="small" 
                        variant="outlined" 
                        sx={{ mb: 1 }} 
                      />
                    )}

                    {note.tags && (
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mb: 1 }}>
                        {note.tags.split(',').map((tag, i) => (
                          <Chip key={i} label={tag.trim()} size="small" variant="outlined" sx={{ fontSize: 10 }} />
                        ))}
                      </Box>
                    )}

                    <Divider sx={{ my: 1 }} />

                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {note.createdByUser && (
                          <>
                            <Avatar sx={{ width: 20, height: 20, fontSize: 10, backgroundColor: '#6750A4' }}>
                              {note.createdByUser.firstName?.[0]}{note.createdByUser.lastName?.[0]}
                            </Avatar>
                            <Typography variant="caption" color="textSecondary">
                              {note.createdByUser.firstName}
                            </Typography>
                          </>
                        )}
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Tooltip title={visibility?.label || 'Private'}>
                          <Box sx={{ color: '#9e9e9e', display: 'flex' }}>
                            {visibility?.icon}
                          </Box>
                        </Tooltip>
                        <Typography variant="caption" color="textSecondary">
                          {new Date(note.createdAt).toLocaleDateString()}
                        </Typography>
                      </Box>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            );
          })}
        </Grid>

        {filteredNotes.length === 0 && (
          <Paper sx={{ p: 4, textAlign: 'center' }}>
            <NoteIcon sx={{ fontSize: 48, color: '#9e9e9e', mb: 2 }} />
            <Typography color="textSecondary">
              No notes found. Create your first note to get started.
            </Typography>
          </Paper>
        )}
      </Container>

      {/* Add/Edit Note Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingId ? 'Edit Note' : 'Add Note'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField fullWidth label="Title (optional)" name="title" value={formData.title} onChange={handleInputChange} />
            </Grid>
            <Grid item xs={4}>
              <LookupSelect
                category="NoteType"
                name="noteType"
                value={formData.noteType}
                onChange={handleSelectChange}
                label="Note Type"
                fallback={NOTE_TYPES.map(t => ({ value: t.value, label: t.label }))}
              />
            </Grid>
            <Grid item xs={4}>
              <LookupSelect
                category="NoteVisibility"
                name="visibility"
                value={formData.visibility}
                onChange={handleSelectChange}
                label="Visibility"
                fallback={NOTE_VISIBILITY.map(v => ({ value: v.value, label: v.label }))}
              />
            </Grid>
            <Grid item xs={4}>
              <LookupSelect
                category="EntityType"
                name="entityType"
                value={formData.entityType}
                onChange={handleSelectChange}
                label="Link to Entity"
                fallback={[{ value: '', label: 'None' }, ...ENTITY_TYPES.map(e => ({ value: e, label: e }))]}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField 
                fullWidth 
                label="Note Content" 
                name="content" 
                value={formData.content} 
                onChange={handleInputChange} 
                multiline 
                rows={8}
                required
                placeholder="Write your note here..."
              />
            </Grid>
            <Grid item xs={6}>
              <TextField 
                fullWidth 
                label="Mentions (usernames, comma-separated)" 
                name="mentions" 
                value={formData.mentions} 
                onChange={handleInputChange} 
                placeholder="@john, @jane"
              />
            </Grid>
            <Grid item xs={6}>
              <TextField 
                fullWidth 
                label="Tags (comma-separated)" 
                name="tags" 
                value={formData.tags} 
                onChange={handleInputChange}
                placeholder="important, follow-up, idea"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveNote} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default NotesPage;
