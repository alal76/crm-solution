import React, { useState, useRef } from 'react';
import fileUploadService from '../services/fileUploadService';
import {
  Box,
  Avatar,
  Button,
  CircularProgress,
  Typography,
  IconButton,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Delete as DeleteIcon,
  Person as PersonIcon,
  Business as BusinessIcon,
} from '@mui/icons-material';

interface ImageUploadProps {
  currentImageUrl?: string | null;
  onImageChange: (url: string | null) => void;
  uploadEndpoint: 'logo' | 'user-photo' | 'customer-logo' | 'contact-photo';
  size?: number;
  placeholder?: 'person' | 'business';
  label?: string;
  initials?: string;
}

const ImageUpload: React.FC<ImageUploadProps> = ({
  currentImageUrl,
  onImageChange,
  uploadEndpoint,
  size = 100,
  placeholder = 'person',
  label = 'Upload Image',
  initials,
}) => {
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState<string | null>(currentImageUrl || null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file size (5MB max)
    if (file.size > 5 * 1024 * 1024) {
      alert('File size must be less than 5MB');
      return;
    }

    // Validate file type
    const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      alert('Invalid file type. Please upload a JPG, PNG, GIF, or WebP image.');
      return;
    }

    setUploading(true);
    try {
      const result = await fileUploadService.uploadFile(uploadEndpoint, file);
      setPreview(result.url);
      onImageChange(result.url);
    } catch (err) {
      console.error('Error uploading image:', err);
      alert('Failed to upload image');
    } finally {
      setUploading(false);
    }
  };

  const handleRemove = async () => {
    if (preview && preview.startsWith('/uploads')) {
      try {
        await fileUploadService.deleteFile(preview);
      } catch (err) {
        console.error('Error deleting image:', err);
      }
    }
    setPreview(null);
    onImageChange(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const PlaceholderIcon = placeholder === 'business' ? BusinessIcon : PersonIcon;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 1 }}>
      <Box sx={{ position: 'relative' }}>
        <Avatar
          src={preview || undefined}
          sx={{
            width: size,
            height: size,
            backgroundColor: '#E8DEF8',
            fontSize: size * 0.35,
          }}
        >
          {!preview && (initials || <PlaceholderIcon sx={{ fontSize: size * 0.5, color: '#6750A4' }} />)}
        </Avatar>
        {uploading && (
          <Box
            sx={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              bottom: 0,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              backgroundColor: 'rgba(255,255,255,0.7)',
              borderRadius: '50%',
            }}
          >
            <CircularProgress size={size * 0.4} />
          </Box>
        )}
        {preview && !uploading && (
          <IconButton
            size="small"
            onClick={handleRemove}
            sx={{
              position: 'absolute',
              top: -8,
              right: -8,
              backgroundColor: 'error.main',
              color: 'white',
              '&:hover': { backgroundColor: 'error.dark' },
              width: 24,
              height: 24,
            }}
          >
            <DeleteIcon sx={{ fontSize: 14 }} />
          </IconButton>
        )}
      </Box>

      <Button
        variant="outlined"
        component="label"
        size="small"
        startIcon={<UploadIcon />}
        disabled={uploading}
        sx={{
          textTransform: 'none',
          color: '#6750A4',
          borderColor: '#6750A4',
          fontSize: '0.75rem',
        }}
      >
        {label}
        <input
          ref={fileInputRef}
          hidden
          accept="image/*"
          type="file"
          onChange={handleFileChange}
        />
      </Button>
      <Typography variant="caption" sx={{ color: '#79747E' }}>
        Max 5MB (JPG, PNG, GIF, WebP)
      </Typography>
    </Box>
  );
};

export default ImageUpload;
