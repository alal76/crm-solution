import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
  Avatar,
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
  IconButton,
  List,
  ListItem,
  Paper,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import ReplyIcon from '@mui/icons-material/Reply';
import SendIcon from '@mui/icons-material/Send';
import CheckIcon from '@mui/icons-material/Check';
import CloseIcon from '@mui/icons-material/Close';
import apiClient from '../../services/apiClient';
import {
  createComment,
  deleteComment,
  getCommentsByEntity,
  RecordCommentItem,
  updateComment,
} from '../../services/recordCommentService';

// ── Types ─────────────────────────────────────────────────────────────────────

export interface RecordCommentsProps {
  entityType: string;
  entityId: number;
}

interface UserHint {
  id: number;
  fullName: string;
  username: string;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function getInitials(name: string): string {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((n) => n[0].toUpperCase())
    .join('');
}

function relativeTime(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'just now';
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(dateStr).toLocaleDateString();
}

function parseMentionedIds(raw?: string | null): number[] {
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? (parsed as number[]) : [];
  } catch {
    return [];
  }
}

/**
 * Extract @mention query from text: returns the partial name after the last `@`
 * if the cursor is inside a mention token, otherwise null.
 */
function getMentionQuery(text: string): string | null {
  const atIdx = text.lastIndexOf('@');
  if (atIdx < 0) return null;
  const after = text.slice(atIdx + 1);
  // Stop if there's a space (mention completed)
  if (after.includes(' ')) return null;
  return after;
}

// ── Sub-component: CommentCompose ─────────────────────────────────────────────

interface ComposeBoxProps {
  placeholder?: string;
  onSubmit: (content: string, mentionedIds: number[]) => Promise<void>;
  onCancel?: () => void;
  autoFocus?: boolean;
}

const CommentCompose: React.FC<ComposeBoxProps> = ({
  placeholder = 'Write a comment… (Ctrl+Enter to submit)',
  onSubmit,
  onCancel,
  autoFocus = false,
}) => {
  const theme = useTheme();
  const [text, setText] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [mentionedIds, setMentionedIds] = useState<number[]>([]);
  const [mentionQuery, setMentionQuery] = useState<string | null>(null);
  const [userHints, setUserHints] = useState<UserHint[]>([]);
  const inputRef = useRef<HTMLInputElement>(null);

  // Debounce @mention user search
  useEffect(() => {
    if (mentionQuery === null || mentionQuery.length < 1) {
      setUserHints([]);
      return;
    }
    const timer = setTimeout(async () => {
      try {
        const res = await apiClient.get<UserHint[]>('/users', {
          params: { search: mentionQuery, pageSize: 8 },
        });
        // API may return a paged object; extract items robustly
        const data = res.data;
        const items: UserHint[] = Array.isArray(data)
          ? data
          : Array.isArray((data as any).items)
          ? (data as any).items
          : [];
        setUserHints(
          items.map((u: any) => ({
            id: u.id,
            fullName: u.fullName ?? `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim(),
            username: u.username ?? u.email ?? '',
          })),
        );
      } catch {
        setUserHints([]);
      }
    }, 250);
    return () => clearTimeout(timer);
  }, [mentionQuery]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setText(val);
    setMentionQuery(getMentionQuery(val));
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && e.ctrlKey) {
      e.preventDefault();
      handleSubmit();
    }
    if (e.key === 'Escape' && onCancel) onCancel();
  };

  const insertMention = (user: UserHint) => {
    const atIdx = text.lastIndexOf('@');
    const newText = `${text.slice(0, atIdx)}@${user.fullName} `;
    setText(newText);
    setMentionedIds((prev) => (prev.includes(user.id) ? prev : [...prev, user.id]));
    setUserHints([]);
    setMentionQuery(null);
    inputRef.current?.focus();
  };

  const handleSubmit = async () => {
    const trimmed = text.trim();
    if (!trimmed) return;
    setSubmitting(true);
    try {
      await onSubmit(trimmed, mentionedIds);
      setText('');
      setMentionedIds([]);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ position: 'relative' }}>
      <TextField
        inputRef={inputRef}
        fullWidth
        multiline
        minRows={2}
        maxRows={6}
        placeholder={placeholder}
        value={text}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        autoFocus={autoFocus}
        disabled={submitting}
        size="small"
        sx={{ mb: 1 }}
      />

      {/* @mention autocomplete popover */}
      {userHints.length > 0 && (
        <Paper
          elevation={4}
          sx={{
            position: 'absolute',
            zIndex: 1400,
            top: '100%',
            left: 0,
            right: 0,
            maxHeight: 200,
            overflow: 'auto',
            border: `1px solid ${theme.palette.divider}`,
          }}
        >
          {userHints.map((u) => (
            <Box
              key={u.id}
              sx={{
                px: 2,
                py: 1,
                cursor: 'pointer',
                '&:hover': { backgroundColor: theme.palette.action.hover },
              }}
              onMouseDown={(e) => {
                e.preventDefault();
                insertMention(u);
              }}
            >
              <Typography variant="body2" fontWeight={600}>
                {u.fullName}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                @{u.username}
              </Typography>
            </Box>
          ))}
        </Paper>
      )}

      {/* Mentioned chips */}
      {mentionedIds.length > 0 && (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 1 }}>
          {mentionedIds.map((id) => (
            <Chip
              key={id}
              label={`@${id}`}
              size="small"
              onDelete={() => setMentionedIds((prev) => prev.filter((x) => x !== id))}
            />
          ))}
        </Box>
      )}

      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        {onCancel && (
          <Button size="small" variant="text" onClick={onCancel} disabled={submitting}>
            Cancel
          </Button>
        )}
        <Button
          size="small"
          variant="contained"
          endIcon={submitting ? <CircularProgress size={14} /> : <SendIcon fontSize="inherit" />}
          onClick={handleSubmit}
          disabled={submitting || !text.trim()}
        >
          {submitting ? 'Sending…' : 'Send'}
        </Button>
      </Box>
    </Box>
  );
};

// ── Sub-component: CommentRow ─────────────────────────────────────────────────

interface CommentRowProps {
  comment: RecordCommentItem;
  depth?: number;
  onReplySubmit: (content: string, mentionedIds: number[], parentId: number) => Promise<void>;
  onEdit: (id: number, content: string, mentionedIds: number[]) => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}

const CommentRow: React.FC<CommentRowProps> = ({
  comment,
  depth = 0,
  onReplySubmit,
  onEdit,
  onDelete,
}) => {
  const theme = useTheme();
  const [replyOpen, setReplyOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editText, setEditText] = useState(comment.content);
  const [saving, setSaving] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);

  const bgColor =
    depth === 0
      ? theme.palette.background.paper
      : theme.palette.mode === 'dark'
      ? 'rgba(255,255,255,0.04)'
      : 'rgba(0,0,0,0.02)';

  const handleEditSave = async () => {
    if (!editText.trim()) return;
    setSaving(true);
    try {
      await onEdit(comment.id, editText.trim(), parseMentionedIds(comment.mentionedUserIds));
      setEditOpen(false);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!confirmDelete) {
      setConfirmDelete(true);
      setTimeout(() => setConfirmDelete(false), 3000);
      return;
    }
    await onDelete(comment.id);
  };

  return (
    <Box
      sx={{
        pl: depth > 0 ? 4 : 0,
        borderLeft: depth > 0 ? `2px solid ${theme.palette.divider}` : 'none',
        ml: depth > 0 ? 2 : 0,
      }}
    >
      <ListItem
        alignItems="flex-start"
        sx={{ px: 0, pb: 1, backgroundColor: bgColor, borderRadius: 1, mb: 0.5 }}
      >
        <Box sx={{ display: 'flex', gap: 1.5, width: '100%', px: 1 }}>
          {/* Avatar */}
          <Avatar
            sx={{
              width: 32,
              height: 32,
              fontSize: 12,
              bgcolor: `hsl(${(comment.authorId * 37) % 360}, 60%, 50%)`,
              flexShrink: 0,
              mt: 0.5,
            }}
            src={comment.authorAvatarUrl ?? undefined}
          >
            {getInitials(comment.authorName)}
          </Avatar>

          {/* Body */}
          <Box sx={{ flex: 1, minWidth: 0 }}>
            {/* Header row */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
              <Typography variant="subtitle2" fontWeight={700} noWrap>
                {comment.authorName}
              </Typography>
              <Tooltip title={new Date(comment.createdAt).toLocaleString()}>
                <Typography variant="caption" color="text.secondary">
                  {relativeTime(comment.createdAt)}
                </Typography>
              </Tooltip>
              {comment.updatedAt && comment.updatedAt !== comment.createdAt && (
                <Typography variant="caption" color="text.disabled">
                  (edited)
                </Typography>
              )}
            </Box>

            {/* Comment text or edit box */}
            {editOpen ? (
              <Box sx={{ mt: 0.5 }}>
                <TextField
                  fullWidth
                  multiline
                  minRows={2}
                  size="small"
                  value={editText}
                  onChange={(e) => setEditText(e.target.value)}
                  autoFocus
                />
                <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                  <IconButton size="small" onClick={handleEditSave} disabled={saving}>
                    {saving ? <CircularProgress size={14} /> : <CheckIcon fontSize="small" />}
                  </IconButton>
                  <IconButton size="small" onClick={() => { setEditOpen(false); setEditText(comment.content); }}>
                    <CloseIcon fontSize="small" />
                  </IconButton>
                </Box>
              </Box>
            ) : (
              <Typography
                variant="body2"
                sx={{ mt: 0.25, whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}
              >
                {comment.content}
              </Typography>
            )}

            {/* Actions row */}
            {!editOpen && (
              <Box sx={{ display: 'flex', gap: 0.5, mt: 0.5, alignItems: 'center' }}>
                {depth === 0 && (
                  <Button
                    size="small"
                    startIcon={<ReplyIcon fontSize="inherit" />}
                    onClick={() => setReplyOpen((v) => !v)}
                    sx={{ minWidth: 0, px: 1, py: 0, fontSize: '0.72rem' }}
                  >
                    Reply
                  </Button>
                )}
                {comment.canEdit && (
                  <IconButton size="small" onClick={() => setEditOpen(true)}>
                    <EditIcon sx={{ fontSize: 14 }} />
                  </IconButton>
                )}
                {comment.canDelete && (
                  <Tooltip title={confirmDelete ? 'Click again to confirm delete' : 'Delete'}>
                    <IconButton
                      size="small"
                      color={confirmDelete ? 'error' : 'default'}
                      onClick={handleDelete}
                    >
                      <DeleteIcon sx={{ fontSize: 14 }} />
                    </IconButton>
                  </Tooltip>
                )}
              </Box>
            )}
          </Box>
        </Box>
      </ListItem>

      {/* Reply compose box */}
      {replyOpen && (
        <Box sx={{ pl: 6, mb: 1 }}>
          <CommentCompose
            placeholder="Write a reply… (Ctrl+Enter to submit)"
            autoFocus
            onCancel={() => setReplyOpen(false)}
            onSubmit={async (content, mentionIds) => {
              await onReplySubmit(content, mentionIds, comment.id);
              setReplyOpen(false);
            }}
          />
        </Box>
      )}

      {/* Replies */}
      {comment.replies && comment.replies.length > 0 && (
        <List disablePadding>
          {comment.replies.map((reply) => (
            <CommentRow
              key={reply.id}
              comment={reply}
              depth={depth + 1}
              onReplySubmit={onReplySubmit}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </List>
      )}
    </Box>
  );
};

// ── Main export: RecordComments ────────────────────────────────────────────────

/**
 * Threaded comment thread for any CRM entity.
 *
 * Usage:
 * ```tsx
 * <RecordComments entityType="Account" entityId={account.id} />
 * ```
 */
export const RecordComments: React.FC<RecordCommentsProps> = ({ entityType, entityId }) => {
  const theme = useTheme();
  const [comments, setComments] = useState<RecordCommentItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchComments = useCallback(async () => {
    if (!entityId) return;
    setLoading(true);
    setError(null);
    try {
      const data = await getCommentsByEntity(entityType, entityId);
      setComments(data);
    } catch {
      setError('Failed to load comments.');
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    fetchComments();
  }, [fetchComments]);

  const handleCreate = async (content: string, mentionedIds: number[]) => {
    await createComment({
      entityType,
      entityId,
      content,
      mentionedUserIds: mentionedIds.length ? JSON.stringify(mentionedIds) : null,
    });
    await fetchComments();
  };

  const handleReply = async (content: string, mentionedIds: number[], parentId: number) => {
    await createComment({
      entityType,
      entityId,
      content,
      parentCommentId: parentId,
      mentionedUserIds: mentionedIds.length ? JSON.stringify(mentionedIds) : null,
    });
    await fetchComments();
  };

  const handleEdit = async (id: number, content: string, mentionedIds: number[]) => {
    await updateComment(id, {
      content,
      mentionedUserIds: mentionedIds.length ? JSON.stringify(mentionedIds) : null,
    });
    await fetchComments();
  };

  const handleDelete = async (id: number) => {
    await deleteComment(id);
    await fetchComments();
  };

  return (
    <Box>
      <Typography
        variant="subtitle1"
        fontWeight={700}
        sx={{ mb: 2, color: theme.palette.text.primary }}
      >
        Comments
      </Typography>

      {/* Compose top-level comment */}
      <Paper
        variant="outlined"
        sx={{ p: 2, mb: 3, borderRadius: 2, backgroundColor: theme.palette.background.default }}
      >
        <CommentCompose onSubmit={handleCreate} />
      </Paper>

      {/* Comment list */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress size={28} />
        </Box>
      ) : error ? (
        <Typography color="error" variant="body2">
          {error}
        </Typography>
      ) : comments.length === 0 ? (
        <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
          No comments yet. Be the first to comment!
        </Typography>
      ) : (
        <List disablePadding>
          {comments.map((comment, index) => (
            <React.Fragment key={comment.id}>
              <CommentRow
                comment={comment}
                onReplySubmit={handleReply}
                onEdit={handleEdit}
                onDelete={handleDelete}
              />
              {index < comments.length - 1 && <Divider sx={{ my: 1 }} />}
            </React.Fragment>
          ))}
        </List>
      )}
    </Box>
  );
};

export default RecordComments;
