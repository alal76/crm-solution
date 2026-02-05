import React from 'react';
import {
  Box, Paper, Typography, Chip, IconButton, Tooltip, Avatar,
  Link, Collapse, Stack
} from '@mui/material';
import {
  Chat as ChatIcon,
  WhatsApp as WhatsAppIcon,
  Facebook as FacebookIcon,
  Instagram as InstagramIcon,
  Sms as SmsIcon,
  Email as EmailIcon,
  Web as WebIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  OpenInNew as OpenInNewIcon
} from '@mui/icons-material';

/**
 * Represents a chat message in the activity timeline
 */
export interface ChatMessageActivity {
  id: number;
  activityType: string;
  activityTypeName: string;
  title: string;
  description?: string;
  details?: string;
  activityDate: string;
  createdAt: string;
  accountId?: number;
  contactId?: number;
  entityType?: string;
  entityId?: number;
  entityName?: string;
  userName?: string;
  externalId?: string;
  externalSource?: string;
}

/**
 * Parsed details from a chat activity
 */
interface ChatDetails {
  chatwootConversationId?: number;
  chatwootMessageId?: number;
  intercomConversationId?: string;
  channel?: string;
  direction?: string;
  agentName?: string;
  fullMessage?: string;
  attachments?: string[];
}

interface ChatTimelineItemProps {
  activity: ChatMessageActivity;
  onViewConversation?: (conversationId: string, source: string) => void;
  chatwootUrl?: string;
  intercomAppId?: string;
}

/**
 * Get the icon component for a chat channel
 */
const getChannelIcon = (channel?: string): React.ReactElement => {
  switch (channel?.toLowerCase()) {
    case 'whatsapp':
      return <WhatsAppIcon sx={{ color: '#25D366' }} />;
    case 'facebook':
    case 'messenger':
      return <FacebookIcon sx={{ color: '#1877F2' }} />;
    case 'instagram':
      return <InstagramIcon sx={{ color: '#E4405F' }} />;
    case 'sms':
      return <SmsIcon sx={{ color: '#607D8B' }} />;
    case 'email':
      return <EmailIcon sx={{ color: '#EA4335' }} />;
    case 'web':
    case 'website':
      return <WebIcon sx={{ color: '#2196F3' }} />;
    default:
      return <ChatIcon sx={{ color: '#9C27B0' }} />;
  }
};

/**
 * Get a human-readable label for the channel
 */
const getChannelLabel = (channel?: string): string => {
  switch (channel?.toLowerCase()) {
    case 'whatsapp':
      return 'WhatsApp';
    case 'facebook':
    case 'messenger':
      return 'Messenger';
    case 'instagram':
      return 'Instagram DM';
    case 'sms':
      return 'SMS';
    case 'email':
      return 'Email';
    case 'web':
    case 'website':
      return 'Web Chat';
    default:
      return channel || 'Chat';
  }
};

/**
 * Format time for display
 */
const formatTimeAgo = (dateString: string): string => {
  const now = new Date();
  const date = new Date(dateString);
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return date.toLocaleDateString();
};

/**
 * ChatTimelineItem component for displaying chat messages in the activity timeline.
 * Supports multiple chat providers (Chatwoot, Intercom) and channels (WhatsApp, Facebook, etc.)
 */
const ChatTimelineItem: React.FC<ChatTimelineItemProps> = ({
  activity,
  onViewConversation,
  chatwootUrl,
  intercomAppId
}) => {
  const [expanded, setExpanded] = React.useState(false);
  
  // Parse chat details from activity
  const chatDetails = React.useMemo<ChatDetails>(() => {
    if (!activity.details) return {};
    try {
      return JSON.parse(activity.details);
    } catch {
      return {};
    }
  }, [activity.details]);

  const isIncoming = chatDetails.direction === 'incoming';
  const hasFullMessage = chatDetails.fullMessage && chatDetails.fullMessage.length > (activity.description?.length || 0);
  
  // Build conversation URL for external provider
  const getConversationUrl = (): string | null => {
    if (activity.externalSource === 'Chatwoot' && chatDetails.chatwootConversationId && chatwootUrl) {
      return `${chatwootUrl}/app/accounts/1/conversations/${chatDetails.chatwootConversationId}`;
    }
    if (activity.externalSource === 'Intercom' && chatDetails.intercomConversationId && intercomAppId) {
      return `https://app.intercom.com/a/apps/${intercomAppId}/inbox/conversation/${chatDetails.intercomConversationId}`;
    }
    return null;
  };

  const conversationUrl = getConversationUrl();

  return (
    <Paper 
      elevation={1} 
      sx={{ 
        p: 2,
        borderLeft: 4,
        borderColor: isIncoming ? 'info.main' : 'success.main',
        backgroundColor: isIncoming ? 'rgba(33, 150, 243, 0.04)' : 'rgba(76, 175, 80, 0.04)'
      }}
    >
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {getChannelIcon(chatDetails.channel)}
          <Typography variant="subtitle2" fontWeight={600}>
            {activity.title}
          </Typography>
          <Chip 
            label={getChannelLabel(chatDetails.channel)}
            size="small"
            variant="outlined"
            sx={{ height: 20, fontSize: '0.7rem' }}
          />
          {isIncoming ? (
            <Chip label="Incoming" size="small" color="info" sx={{ height: 20, fontSize: '0.7rem' }} />
          ) : (
            <Chip label="Outgoing" size="small" color="success" sx={{ height: 20, fontSize: '0.7rem' }} />
          )}
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <Typography variant="caption" color="text.secondary">
            {formatTimeAgo(activity.activityDate || activity.createdAt)}
          </Typography>
          {conversationUrl && (
            <Tooltip title="View full conversation">
              <IconButton 
                size="small" 
                onClick={() => window.open(conversationUrl, '_blank')}
              >
                <OpenInNewIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      </Box>

      {/* Message Content */}
      <Box sx={{ pl: 4 }}>
        <Typography variant="body2" color="text.primary">
          {expanded && chatDetails.fullMessage ? chatDetails.fullMessage : activity.description}
        </Typography>

        {hasFullMessage && (
          <Box 
            onClick={() => setExpanded(!expanded)}
            sx={{ 
              cursor: 'pointer', 
              display: 'flex', 
              alignItems: 'center',
              color: 'primary.main',
              mt: 0.5
            }}
          >
            <Typography variant="caption">
              {expanded ? 'Show less' : 'Show more'}
            </Typography>
            {expanded ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
          </Box>
        )}

        {/* Attachments */}
        {chatDetails.attachments && chatDetails.attachments.length > 0 && (
          <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
            {chatDetails.attachments.map((url, index) => (
              <Chip
                key={index}
                label={`Attachment ${index + 1}`}
                size="small"
                component="a"
                href={url}
                target="_blank"
                clickable
                variant="outlined"
              />
            ))}
          </Stack>
        )}

        {/* Footer: Agent/Sender info and entity link */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {(chatDetails.agentName || activity.userName) && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <Avatar sx={{ width: 20, height: 20, fontSize: 10 }}>
                  {(chatDetails.agentName || activity.userName || '?')[0]}
                </Avatar>
                <Typography variant="caption" color="text.secondary">
                  {chatDetails.agentName || activity.userName}
                </Typography>
              </Box>
            )}
          </Box>
          {activity.entityName && (
            <Chip
              label={`${activity.entityType}: ${activity.entityName}`}
              size="small"
              variant="outlined"
              sx={{ height: 20, fontSize: '0.7rem' }}
            />
          )}
        </Box>
      </Box>
    </Paper>
  );
};

export default ChatTimelineItem;
