import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Box, Typography, Paper, CircularProgress, List, ListItem, ListItemText } from '@mui/material';
import apiClient from '../../services/apiClient';

const CMDBImpactAnalysisPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [impacts, setImpacts] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/api/cmdb/${id}/impact-analysis`);
        setImpacts(response.data ?? []);
      } catch (error) {
        console.error('Failed to load impact analysis', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>
        Impact Analysis
      </Typography>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <CircularProgress />
        ) : impacts.length === 0 ? (
          <Typography color="text.secondary">No impacts found.</Typography>
        ) : (
          <List>
            {impacts.map((impact, index) => (
              <ListItem key={index}>
                <ListItemText primary={impact} />
              </ListItem>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default CMDBImpactAnalysisPage;
