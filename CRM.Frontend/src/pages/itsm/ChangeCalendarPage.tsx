import React, { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import CircularProgress from '@mui/material/CircularProgress';
import apiClient from '../../services/apiClient';

interface ChangeCalendarItem {
  changeId: number;
  number: string;
  shortDescription: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
}

const ChangeCalendarPage: React.FC = () => {
  const [items, setItems] = useState<ChangeCalendarItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const params = new URLSearchParams({
          pageNumber: '1',
          pageSize: '50'
        });
        const response = await apiClient.get(`/api/changes?${params}`);
        setItems(response.data.items ?? response.data);
      } catch (error) {
        console.error('Failed to load change calendar', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Change Calendar</Typography>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            {items.length === 0 ? (
              <Typography color="text.secondary">No scheduled changes found.</Typography>
            ) : (
              items.map((change) => (
                <Paper key={change.changeId} variant="outlined" sx={{ p: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Box>
                      <Typography variant="body2" color="text.secondary">{change.number}</Typography>
                      <Typography fontWeight={500}>{change.shortDescription}</Typography>
                    </Box>
                    <Typography variant="body2" color="text.secondary">
                      {change.plannedStartDate ? new Date(change.plannedStartDate).toLocaleString() : '—'}
                      {' '}→{' '}
                      {change.plannedEndDate ? new Date(change.plannedEndDate).toLocaleString() : '—'}
                    </Typography>
                  </Box>
                </Paper>
              ))
            )}
          </Box>
        )}
      </Paper>
    </Box>
  );
};

export default ChangeCalendarPage;
