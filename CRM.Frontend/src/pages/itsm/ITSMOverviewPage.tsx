import React from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';

const ITSMOverviewPage: React.FC = () => {
  const navigate = useNavigate();

  const cards = [
    { title: 'Incidents', description: 'Track and resolve incidents', action: () => navigate('/itsm/incidents') },
    { title: 'Problems', description: 'Root cause analysis and known errors', action: () => navigate('/itsm/problems') },
    { title: 'Changes', description: 'Plan, approve, and schedule changes', action: () => navigate('/itsm/changes') },
    { title: 'CMDB', description: 'Configuration items and relationships', action: () => navigate('/itsm/cmdb') },
    { title: 'Knowledge Base', description: 'Search and manage articles', action: () => navigate('/itsm/knowledge') },
    { title: 'Service Catalog', description: 'Request and fulfill services', action: () => navigate('/itsm/catalog') }
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">ITSM Overview</Typography>
        <Button variant="contained" onClick={() => navigate('/itsm/metrics')}>View Metrics</Button>
      </Box>

      <Grid container spacing={3}>
        {cards.map((card) => (
          <Grid item xs={12} md={6} xl={4} key={card.title}>
            <Card variant="outlined" sx={{ '&:hover': { boxShadow: 3 }, transition: 'box-shadow 0.2s' }}>
              <CardActionArea onClick={card.action} sx={{ p: 3 }}>
                <Typography variant="h6" fontWeight="bold" gutterBottom>{card.title}</Typography>
                <Typography variant="body2" color="text.secondary">{card.description}</Typography>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={3} sx={{ mt: 2 }}>
        <Grid item xs={12} lg={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>SLA Health</Typography>
            <Typography variant="body2" color="text.secondary">Track response and resolution compliance.</Typography>
            <Button size="small" onClick={() => navigate('/itsm/sla')} sx={{ mt: 2 }}>Open SLA Dashboard →</Button>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Change Calendar</Typography>
            <Typography variant="body2" color="text.secondary">Review upcoming changes and blackout windows.</Typography>
            <Button size="small" onClick={() => navigate('/itsm/changes/calendar')} sx={{ mt: 2 }}>Open Calendar →</Button>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Knowledge Authoring</Typography>
            <Typography variant="body2" color="text.secondary">Create and curate knowledge articles.</Typography>
            <Button size="small" onClick={() => navigate('/itsm/knowledge/editor')} sx={{ mt: 2 }}>Open Editor →</Button>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ITSMOverviewPage;
