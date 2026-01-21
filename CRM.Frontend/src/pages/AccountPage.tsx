import React, { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Box, Button, Typography, Paper } from '@mui/material';
import accountService from '../services/accountService';

const AccountPage: React.FC = () => {
  const { id } = useParams();
  const accountId = Number(id || 0);
  const [file, setFile] = useState<File | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0] ?? null;
    setFile(f);
  };

  const handleUpload = async () => {
    if (!file) return setStatus('Select a file first');
    setStatus('Uploading...');
    try {
      const res = await accountService.uploadContract(accountId, file);
      setStatus('Upload successful');
    } catch (err: any) {
      setStatus(err?.response?.data?.message || 'Upload failed');
    }
  };

  const handleDownload = () => {
    const url = accountService.downloadContractUrl(accountId);
    window.open(url, '_blank');
  };

  const handleDelete = async () => {
    setStatus('Deleting...');
    try {
      await accountService.deleteContract(accountId);
      setStatus('Deleted');
    } catch (err: any) {
      setStatus(err?.response?.data?.message || 'Delete failed');
    }
  };

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6">Account {accountId} — Contract</Typography>
      <Box sx={{ mt: 2 }}>
        <input type="file" onChange={handleFileChange} />
      </Box>
      <Box sx={{ mt: 2, display: 'flex', gap: 2 }}>
        <Button variant="contained" onClick={handleUpload} disabled={!file}>Upload</Button>
        <Button variant="outlined" onClick={handleDownload}>Download</Button>
        <Button variant="text" color="error" onClick={handleDelete}>Delete</Button>
        <Button variant="text" onClick={() => navigate(-1)}>Back</Button>
      </Box>
      {status && <Box sx={{ mt: 2 }}>{status}</Box>}
    </Paper>
  );
};

export default AccountPage;
