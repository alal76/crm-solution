import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  MenuItem,
  Select,
  TextField,
  Tooltip,
  Typography,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress,
  InputLabel,
  FormControl,
  Link,
  Avatar,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Star as StarIcon,
  StarOutline as StarOutlineIcon,
  VerifiedUser as VerifiedIcon,
  OpenInNew as OpenInNewIcon,
  LinkedIn as LinkedInIcon,
  Twitter as TwitterIcon,
  Facebook as FacebookIcon,
  Instagram as InstagramIcon,
  YouTube as YouTubeIcon,
  GitHub as GitHubIcon,
  Telegram as TelegramIcon,
  Public as WebIcon,
} from '@mui/icons-material';
import { 
  contactInfoService, 
  EntityType, 
  LinkedSocialMediaDto, 
  CreateSocialMediaAccountDto, 
  SocialMediaPlatform,
  SocialMediaAccountType 
} from '../../services/contactInfoService';

interface SocialMediaManagerProps {
  entityType: EntityType;
  entityId: number;
  readOnly?: boolean;
  onSocialMediaChange?: () => void;
}

const PLATFORMS: SocialMediaPlatform[] = [
  'Website', 'LinkedIn', 'Twitter', 'Facebook', 'Instagram', 'YouTube', 
  'TikTok', 'Pinterest', 'WhatsApp', 'Telegram', 'WeChat', 
  'Slack', 'Discord', 'GitHub', 'Other'
];

const ACCOUNT_TYPES: SocialMediaAccountType[] = ['Personal', 'Business', 'Official', 'Support'];

const SocialMediaManager: React.FC<SocialMediaManagerProps> = ({
  entityType,
  entityId,
  readOnly = false,
  onSocialMediaChange,
}) => {
  const [accounts, setAccounts] = useState<LinkedSocialMediaDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState<LinkedSocialMediaDto | null>(null);
  const [formData, setFormData] = useState<CreateSocialMediaAccountDto & { isPrimary: boolean; preferredForContact: boolean }>({
    platform: 'Website',
    platformOther: '',
    accountType: 'Business',
    handleOrUsername: '',
    profileUrl: '',
    displayName: '',
    followerCount: undefined,
    followingCount: undefined,
    isVerifiedAccount: false,
    notes: '',
    isPrimary: false,
    preferredForContact: false,
  });

  const fetchAccounts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getSocialMediaAccounts(entityType, entityId);
      setAccounts(response.data || []);
    } catch (err) {
      setError('Failed to load social media accounts');
      console.error('Error fetching social media:', err);
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    if (entityId) {
      fetchAccounts();
    }
  }, [entityId, fetchAccounts]);

  const handleOpenDialog = (account?: LinkedSocialMediaDto) => {
    if (account) {
      setEditingAccount(account);
      setFormData({
        platform: account.platform,
        platformOther: account.platformOther || '',
        accountType: account.accountType,
        handleOrUsername: account.handleOrUsername,
        profileUrl: account.profileUrl || '',
        displayName: account.displayName || '',
        followerCount: account.followerCount,
        followingCount: account.followingCount,
        isVerifiedAccount: account.isVerifiedAccount || false,
        notes: account.notes || '',
        isPrimary: account.isPrimary,
        preferredForContact: account.preferredForContact,
      });
    } else {
      setEditingAccount(null);
      setFormData({
        platform: 'Website',
        platformOther: '',
        accountType: 'Business',
        handleOrUsername: '',
        profileUrl: '',
        displayName: '',
        followerCount: undefined,
        followingCount: undefined,
        isVerifiedAccount: false,
        notes: '',
        isPrimary: accounts.length === 0,
        preferredForContact: false,
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingAccount(null);
  };

  const handleSave = async () => {
    try {
      const { isPrimary, preferredForContact, ...accountData } = formData;

      if (editingAccount) {
        await contactInfoService.updateSocialMediaAccount(editingAccount.id, accountData);
        if (isPrimary && !editingAccount.isPrimary) {
          await contactInfoService.setPrimarySocialMedia(entityType, entityId, editingAccount.id);
        }
      } else {
        await contactInfoService.linkSocialMediaAccount({
          newSocialMedia: accountData,
          entityType,
          entityId,
          isPrimary,
          preferredForContact,
        });
      }

      await fetchAccounts();
      handleCloseDialog();
      onSocialMediaChange?.();
    } catch (err) {
      setError('Failed to save social media account');
      console.error('Error saving social media:', err);
    }
  };

  const handleDelete = async (account: LinkedSocialMediaDto) => {
    if (window.confirm('Are you sure you want to remove this social media account?')) {
      try {
        await contactInfoService.unlinkSocialMediaAccount(account.linkId);
        await fetchAccounts();
        onSocialMediaChange?.();
      } catch (err) {
        setError('Failed to remove social media account');
        console.error('Error deleting social media:', err);
      }
    }
  };

  const handleSetPrimary = async (account: LinkedSocialMediaDto) => {
    try {
      await contactInfoService.setPrimarySocialMedia(entityType, entityId, account.id);
      await fetchAccounts();
      onSocialMediaChange?.();
    } catch (err) {
      setError('Failed to set primary social media');
      console.error('Error setting primary social media:', err);
    }
  };

  const getPlatformIcon = (platform: SocialMediaPlatform) => {
    switch (platform) {
      case 'Website':
        return <WebIcon fontSize="small" sx={{ color: '#4CAF50' }} />;
      case 'LinkedIn':
        return <LinkedInIcon fontSize="small" sx={{ color: '#0077B5' }} />;
      case 'Twitter':
        return <TwitterIcon fontSize="small" sx={{ color: '#1DA1F2' }} />;
      case 'Facebook':
        return <FacebookIcon fontSize="small" sx={{ color: '#4267B2' }} />;
      case 'Instagram':
        return <InstagramIcon fontSize="small" sx={{ color: '#E1306C' }} />;
      case 'YouTube':
        return <YouTubeIcon fontSize="small" sx={{ color: '#FF0000' }} />;
      case 'GitHub':
        return <GitHubIcon fontSize="small" />;
      case 'Telegram':
        return <TelegramIcon fontSize="small" sx={{ color: '#0088CC' }} />;
      default:
        return <WebIcon fontSize="small" />;
    }
  };

  const formatFollowerCount = (count?: number) => {
    if (!count) return '';
    if (count >= 1000000) return `${(count / 1000000).toFixed(1)}M`;
    if (count >= 1000) return `${(count / 1000).toFixed(1)}K`;
    return count.toString();
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  return (
    <Card variant="outlined">
      <CardHeader
        title="Social Media"
        titleTypographyProps={{ variant: 'h6' }}
        action={
          !readOnly && (
            <Button
              startIcon={<AddIcon />}
              size="small"
              onClick={() => handleOpenDialog()}
            >
              Add Account
            </Button>
          )
        }
      />
      <CardContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {accounts.length === 0 ? (
          <Typography variant="body2" color="textSecondary">
            No social media accounts on file
          </Typography>
        ) : (
          <List dense>
            {accounts.map((account) => (
              <ListItem key={account.linkId} divider>
                <Box mr={1}>
                  <Avatar sx={{ width: 32, height: 32, bgcolor: 'transparent' }}>
                    {getPlatformIcon(account.platform)}
                  </Avatar>
                </Box>
                <ListItemText
                  primary={
                    <Box display="flex" alignItems="center" gap={1} flexWrap="wrap">
                      <Typography variant="body1" component="span">
                        {account.displayName || account.handleOrUsername}
                      </Typography>
                      {account.handleOrUsername && account.displayName && (
                        <Typography variant="body2" color="textSecondary" component="span">
                          @{account.handleOrUsername}
                        </Typography>
                      )}
                      <Chip
                        label={account.platformName || account.platform}
                        size="small"
                        variant="outlined"
                      />
                      <Chip
                        label={account.accountType}
                        size="small"
                        variant="outlined"
                      />
                      {account.isPrimary && (
                        <Chip
                          label="Primary"
                          size="small"
                          color="primary"
                        />
                      )}
                      {account.isVerifiedAccount && (
                        <Chip
                          icon={<VerifiedIcon />}
                          label="Verified"
                          size="small"
                          color="info"
                          variant="outlined"
                        />
                      )}
                      {account.preferredForContact && (
                        <Chip
                          label="Preferred for Contact"
                          size="small"
                          color="success"
                          variant="outlined"
                        />
                      )}
                    </Box>
                  }
                  secondary={
                    <Box display="flex" alignItems="center" gap={2} mt={0.5}>
                      {account.profileUrl && (
                        <Link
                          href={account.profileUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}
                        >
                          <OpenInNewIcon fontSize="small" />
                          View Profile
                        </Link>
                      )}
                      {account.followerCount && (
                        <Typography variant="caption" color="textSecondary">
                          {formatFollowerCount(account.followerCount)} followers
                        </Typography>
                      )}
                      {account.engagementLevel && (
                        <Chip
                          label={`Engagement: ${account.engagementLevel}`}
                          size="small"
                          variant="outlined"
                        />
                      )}
                    </Box>
                  }
                />
                {!readOnly && (
                  <ListItemSecondaryAction>
                    {!account.isPrimary && (
                      <Tooltip title="Set as Primary">
                        <IconButton size="small" onClick={() => handleSetPrimary(account)}>
                          <StarOutlineIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {account.isPrimary && (
                      <Tooltip title="Primary Account">
                        <IconButton size="small" disabled>
                          <StarIcon fontSize="small" color="primary" />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => handleOpenDialog(account)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Remove">
                      <IconButton size="small" onClick={() => handleDelete(account)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </ListItemSecondaryAction>
                )}
              </ListItem>
            ))}
          </List>
        )}
      </CardContent>

      {/* Add/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingAccount ? 'Edit Social Media Account' : 'Add Social Media Account'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Platform</InputLabel>
                <Select
                  value={formData.platform}
                  onChange={(e) => setFormData({ ...formData, platform: e.target.value as SocialMediaPlatform })}
                  label="Platform"
                >
                  {PLATFORMS.map((platform) => (
                    <MenuItem key={platform} value={platform}>
                      <Box display="flex" alignItems="center" gap={1}>
                        {getPlatformIcon(platform)}
                        {platform}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {formData.platform === 'Other' && (
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Platform Name"
                  value={formData.platformOther}
                  onChange={(e) => setFormData({ ...formData, platformOther: e.target.value })}
                  size="small"
                />
              </Grid>
            )}

            <Grid item xs={12} sm={formData.platform === 'Other' ? 12 : 6}>
              <FormControl fullWidth size="small">
                <InputLabel>Account Type</InputLabel>
                <Select
                  value={formData.accountType}
                  onChange={(e) => setFormData({ ...formData, accountType: e.target.value as SocialMediaAccountType })}
                  label="Account Type"
                >
                  {ACCOUNT_TYPES.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Handle/Username"
                value={formData.handleOrUsername}
                onChange={(e) => setFormData({ ...formData, handleOrUsername: e.target.value })}
                required
                size="small"
                placeholder="@username"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Profile URL"
                value={formData.profileUrl}
                onChange={(e) => setFormData({ ...formData, profileUrl: e.target.value })}
                size="small"
                placeholder="https://linkedin.com/in/username"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Display Name"
                value={formData.displayName}
                onChange={(e) => setFormData({ ...formData, displayName: e.target.value })}
                size="small"
              />
            </Grid>

            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Followers"
                type="number"
                value={formData.followerCount || ''}
                onChange={(e) => setFormData({ ...formData, followerCount: e.target.value ? parseInt(e.target.value) : undefined })}
                size="small"
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Following"
                type="number"
                value={formData.followingCount || ''}
                onChange={(e) => setFormData({ ...formData, followingCount: e.target.value ? parseInt(e.target.value) : undefined })}
                size="small"
              />
            </Grid>

            <Grid item xs={12}>
              <Box display="flex" flexWrap="wrap" gap={2}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.isVerifiedAccount || false}
                      onChange={(e) => setFormData({ ...formData, isVerifiedAccount: e.target.checked })}
                    />
                  }
                  label="Verified Account"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.isPrimary}
                      onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                    />
                  }
                  label="Primary Account"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.preferredForContact}
                      onChange={(e) => setFormData({ ...formData, preferredForContact: e.target.checked })}
                    />
                  }
                  label="Preferred for Contact"
                />
              </Box>
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                multiline
                rows={2}
                size="small"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSave} variant="contained" color="primary">
            {editingAccount ? 'Save Changes' : 'Add Account'}
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  );
};

export default SocialMediaManager;
